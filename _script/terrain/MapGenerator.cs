////////////////////////////////////////////////////////////////////////////////////////////
/// This script is part of the project "Infinite Runner", a procedural generation project
/// By Adrien Pierret
/// 
/// MapGenerator: This script is where various noises are blended and where the actual terrain is shaped.
/// It would be cool to expose all these to the editor. But I lack the time. You'll notice all these references to .isl files. 
/// These will eventually allow to save the terrain chunks on disk, making them reusable or allowing for persistent deformation.
/// ///////////////////////////////////////////////////////////////////////////////////////
using Godot;
using System.IO;
using Bouncerock;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Bouncerock.Procedural;
using System.Linq;
//using System.Numerics;
namespace Bouncerock.Terrain
{
	public static class MapGenerator
	{



		static string documentspath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
				+ "/Islands/";
		//This is where new chunks are generated and assembled.

		public static async Task<Map> GenerateMapAsync(Vector2 sampleCentre)
		{
			Vector2 offset = new Vector2(
				-(TerrainMeshSettings.numVertsPerLine / 2) + sampleCentre.X,
				(TerrainMeshSettings.numVertsPerLine / 2) - sampleCentre.Y);

			Map map = new Map(Map.Origins.Generated);

			try
			{
				// Base heightmap
				TerrainPass basePass = TerrainManager.Instance.CurrentMapSettings.Passes[0];
				float[,] heightMap = GenerateHeightMapSimplex(
					TerrainMeshSettings.numVertsPerLine,
					TerrainMeshSettings.numVertsPerLine,
					basePass,
					offset, 0);

				heightMap = ApplyContrast(heightMap, basePass.Contrast);

				// Additive passes
				for (int i = 1; i < TerrainManager.Instance.CurrentMapSettings.Passes.Count; i++)
				{
					TerrainPass pass = TerrainManager.Instance.CurrentMapSettings.Passes[i];
					float[,] heightMapTemp = GenerateHeightMapSimplex(
						TerrainMeshSettings.numVertsPerLine,
						TerrainMeshSettings.numVertsPerLine,
						pass,
						offset, i);

					heightMapTemp = ApplyContrast(heightMapTemp, pass.Contrast);
					heightMap = ApplyBlend(heightMap, heightMapTemp, pass);
				}

				map.SetHeightmap(heightMap);

				List<WorldItem> _worldItems = new List<WorldItem>();

				foreach (WorldItemSettings naturalObject in TerrainManager.Instance.CurrentMapSettings.NaturalObjects)
				{
					if (naturalObject.Concentration > 1)
					{
						List<WorldItem> items = await GenerateStaticElements(heightMap, naturalObject);
						if (items.Count > 0)
						{
							_worldItems.AddRange(items);
						}
					}
					else
					{
						WorldItem item = await DetermineItemPresence(heightMap, naturalObject);
						if (item != null && item.ModelAddress != ""){_worldItems.Add(item);}
						
					}
				}

				foreach (WorldItemSettings gameplayObject in TerrainManager.Instance.CurrentMapSettings.GameplayObjects)
				{
					if (gameplayObject.Concentration > 1)
					{
						List<WorldItem> items = await GenerateStaticElements(heightMap, gameplayObject);
						if (items.Count > 0)
						{
							_worldItems.AddRange(items);
						}
					}
					else
					{
						WorldItem item = await DetermineItemPresence(heightMap, gameplayObject);
						if (item != null && item.ModelAddress != ""){_worldItems.Add(item);}
					}
				}
				map.AddTerrainElements(_worldItems);
				//GD.Print("[Step 1] Chunk " + sampleCentre + " generated " + map.GetTerrainElements().Count + " elements");

				return map;
			}
			catch (Exception ex)
			{
				GD.Print("Error in Map Generation: " + ex);
				throw;
			}
		}


		public class Pathway
		{
			public List<Vector2> Points = new List<Vector2>(); // local positions in the current heightmap
			public PathwaySettings Settings;
		}

		private static Pathway CreatePathway(float[,] heightMap, PathwaySettings pathSettings, int seed = 0)
		{
			int size = heightMap.GetLength(0);
			Random random = new Random(seed);

			Pathway pathway = new Pathway();
			pathway.Settings = pathSettings;

			// Start on left edge
			Vector2 pos = new Vector2(0, random.Next(0, size));
			Vector2 direction = new Vector2(1, 0); // initially right
			pathway.Points.Add(pos);

			int maxSteps = size * 2; // prevent infinite loops
			for (int i = 0; i < maxSteps; i++)
			{
				// Simple meander: small random angle change
				float angleOffset = ((float)random.NextDouble() - 0.5f) * 0.5f;
				direction = direction.Rotated(angleOffset).Normalized();

				Vector2 nextPos = pos + direction;

				// Clamp to map
				nextPos.X = Mathf.Clamp(nextPos.X, 0, size - 1);
				nextPos.Y = Mathf.Clamp(nextPos.Y, 0, size - 1);

				float height = heightMap[(int)nextPos.X, (int)nextPos.Y];
				if (height < pathSettings.MinElevation || height > pathSettings.MaxElevation)
				{
					// stop if too steep
					break;
				}

				pos = nextPos;
				pathway.Points.Add(pos);

				// stop if reached right edge
				if (pos.X >= size - 1) break;
			}

			return pathway;
		}

		private static float[,] ApplyBlend(float[,] heightMap1, float[,] heightMap2, TerrainPass pass)
		{
			const float TRANSPARENT = -201f;

			int width = heightMap1.GetLength(0);
			int height = heightMap1.GetLength(1);

			float[,] result = new float[width, height];

			bool hasMask = pass.Mask != null;

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					float a = heightMap1[x, y];
					float b = heightMap2[x, y];

					// --- Transparent handling ---
					if (a == TRANSPARENT)
					{
						result[x, y] = b;
						continue;
					}
					if (b == TRANSPARENT)
					{
						result[x, y] = a;
						continue;
					}

					//  Height-based smooth constraint 
					float heightMask = Mathf.SmoothStep(
						pass.MinHeight,
						pass.MaxHeight,
						a
					);

					// Optional user mask
					float mask = hasMask ? Mathf.Clamp(pass.Mask[x, y], 0f, 1f) : 1f;

					// Final blend weight
					float weight = pass.BlendValue * heightMask * mask;

					float blended;

					switch (pass.BlendType)
					{
						case TerrainPass.BlendTypes.Mix:
							blended = MathExt.Lerp(a, b, weight);
							break;

						case TerrainPass.BlendTypes.Add:
							blended = a + Math.Max(0f, b) * weight;
							break;

						case TerrainPass.BlendTypes.Substract:
							blended = a - Math.Max(0f, b) * weight;
							break;

						case TerrainPass.BlendTypes.None:
						default:
							blended = a;
							break;
					}

					result[x, y] = blended;
				}
			}

			return result;
		}





		private static float[,] ApplyContrast(float[,] heightMap, float contrast, float mid = 0)
		{
			int width = heightMap.GetLength(0);
			int height = heightMap.GetLength(1);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					float delta = heightMap[x, y] - mid;
					heightMap[x, y] = mid + delta * contrast;
				}
			}
			return heightMap;
		}

		/*static async Task<WorldItem> DetermineItemPresence(float[,] heightmap, WorldItemSettings settings)
		{
			WorldItem item = new WorldItem();
			item.settings = settings;
			RandomNumberGenerator rnd = new RandomNumberGenerator();
			rnd.Seed = (ulong)DateTime.Now.ToBinary();
			float chance = rnd.RandfRange(0, 1);
			//GD.Print("Item present " + naturalObject.ObjectName + " chance " + chance);
			if (settings.Concentration >= chance)
			{
				Vector2 location = new Vector2();
				location.X = rnd.RandfRange(0, TerrainMeshSettings.numVertsPerLine);
				location.Y = rnd.RandfRange(0, TerrainMeshSettings.numVertsPerLine);
				item.ItemName = settings.Name;
				item.GridLocation = location;
				return item;
			}
			return item;
		}*/

		//This function is the one that determines the spread of objects in the terrain and all their properties. 
		private static float GetHeightAtChunkMapLocation(float[,] heightMap, Vector2 location)
		{
			try
			{
				// Clamp the x and y indices to ensure they are within bounds.
				int xClamped = Mathf.RoundToInt(location.X);
				int yClamped = Mathf.RoundToInt(location.Y);

				// Access the height map with clamped indices.
				return heightMap[xClamped, yClamped];
			}
			catch (Exception)
			{
				//GD.Print("out of bounds" + location.X +"/"+location.Y);
				// Return -201 if any exception occurs (e.g., out of bounds).
				return -201;
			}
		}

		static async Task<WorldItem> DetermineItemPresence(float[,] heightmap, WorldItemSettings settings)
		{
			WorldItem item = new WorldItem();
			item.GridLocation = Vector2.Zero;
			item.settings = settings;
			item.ModelAddress = "";
			RandomNumberGenerator rnd = new RandomNumberGenerator();
			rnd.Seed = (ulong)DateTime.Now.ToBinary();
			float chance = rnd.RandfRange(0, 1);
			//GD.Print("Item present " + naturalObject.ObjectName + " chance " + chance);
			if (settings.Concentration >= chance)
			{
				Vector2 location = new Vector2();
				location.X = rnd.RandfRange(0, TerrainMeshSettings.numVertsPerLine);
				location.Y = rnd.RandfRange(0, TerrainMeshSettings.numVertsPerLine);

				/////////////////////////////////////////////
				WorldItem itm = new WorldItem();
				itm.settings = settings;

				itm.GridLocation = location;
				//GD.Print("INGRIDLOC " + itm.GridLocation);
				itm.ItemName = settings.Name;
				itm.ModelAddress = settings.Path;
				if (settings.MinSize != settings.MaxSize)
				{
					rnd.Seed = (ulong)location.X;
					itm.Scale = Vector3.One * rnd.RandfRange(settings.MinSize, settings.MaxSize);
				}

				//Excluding conditions
				Vector2 inGridLocation = new Vector2(25 - itm.GridLocation.X, 25 - itm.GridLocation.Y);

				Vector3 worldLocation = new Vector3(-1 * inGridLocation.X, GetHeightAtChunkMapLocation(heightmap, itm.GridLocation), inGridLocation.Y);

				if (worldLocation.Y < settings.MinimumSpawnAltitude || worldLocation.Y > settings.MaximumSpawnAltitude)
				{
					return null;//Discard this.
				}
				float rotZ = 0;
				float rotX = 0;
				float rotY = 0;
				if (settings.RandomizeYRotation)
				{
					float rotation = TerrainManager.Instance.TerrainDetailsRandom.RandfRange(0, 360);
					//GD.Print("rad " + rotation);
					rotY = Mathf.DegToRad(rotation);
					//GD.Print("deg " + rotation);
					//objectToSpawn.relevantItem.RotateY(rotation);

					// item.Model.RotateY(rotation);
				}
				if (settings.RandomizeTiltAngle != 0)
				{

					float rotation = TerrainManager.Instance.TerrainDetailsRandom.RandfRange(0, settings.RandomizeTiltAngle);
					rotation = Mathf.DegToRad(rotation);
					rotZ = rotation;
					//item.Model.RotateZ(rotation);
					//objectToSpawn.relevantItem.RotateZ(rotation);
					//GD.Print(rotation);
					float rotation2 = TerrainManager.Instance.TerrainDetailsRandom.RandfRange(0, settings.RandomizeTiltAngle);
					rotation2 = Mathf.DegToRad(rotation2);
					rotX = rotation2;//item.Model.RotateX(rotation2);
									 // objectToSpawn.relevantItem.RotateX(rotation2);
				}
				itm.Rotation = new Vector3(rotX, rotY, rotZ);
				itm.Scale = Vector3.One * TerrainManager.Instance.TerrainDetailsRandom.RandfRange(settings.MinSize, settings.MaxSize);
				itm.Hash = "";
				return itm;
			}
			return item;
		}

		static async Task<List<WorldItem>> GenerateStaticElements(float[,] heightmap, WorldItemSettings settings)
		{

			//List<Vector2> locations = await PoissonDiscSampling.Test(Vector2.One * (TerrainMeshSettings.numVertsPerLine-3), Vector2.Zero, 10);
			int seed = (int)DateTime.Now.ToBinary();
			//int minSpacing = Math.Clamp(70-(int)naturalObject.Concentration, 5,30);
			List<Vector2> locations = await PoissonDiscSampling.GeneratePoints(Vector2.Zero, 10, Vector2.One * TerrainMeshSettings.numVertsPerLine, (int)settings.Concentration, seed);
			//GD.Print("Poisson disc loc: " + locations.Count + " elements ");

			List<WorldItem> generated = new List<WorldItem>();
			int i = 0;
			try
			{
				foreach (Vector2 location in locations)
				{
					WorldItem itm = new WorldItem();
					itm.settings = settings;

					itm.GridLocation = location;
					
					itm.ModelAddress = settings.Path;
					if (settings.MinSize != settings.MaxSize)
					{
						RandomNumberGenerator rnd = new RandomNumberGenerator();
						rnd.Seed = (ulong)location.X;
						itm.Scale = Vector3.One * rnd.RandfRange(settings.MinSize, settings.MaxSize);
					}

					//Excluding conditions
					Vector2 inGridLocation = new Vector2(25 - itm.GridLocation.X, 25 - itm.GridLocation.Y);

					Vector3 worldLocation = new Vector3(-1 * inGridLocation.X, GetHeightAtChunkMapLocation(heightmap, itm.GridLocation), inGridLocation.Y);

					if (worldLocation.Y < settings.MinimumSpawnAltitude || worldLocation.Y > settings.MaximumSpawnAltitude)
					{
						
						continue;//Discard this.
					}
					float rotZ = 0;
					float rotX = 0;
					float rotY = 0;
					if (settings.RandomizeYRotation)
					{
						float rotation = TerrainManager.Instance.TerrainDetailsRandom.RandfRange(0, 360);
						//GD.Print("rad " + rotation);
						rotY = Mathf.DegToRad(rotation);
						//GD.Print("deg " + rotation);
						//objectToSpawn.relevantItem.RotateY(rotation);

						// item.Model.RotateY(rotation);
					}
					if (settings.RandomizeTiltAngle != 0)
					{

						float rotation = TerrainManager.Instance.TerrainDetailsRandom.RandfRange(0, settings.RandomizeTiltAngle);
						rotation = Mathf.DegToRad(rotation);
						rotZ = rotation;
						//item.Model.RotateZ(rotation);
						//objectToSpawn.relevantItem.RotateZ(rotation);
						//GD.Print(rotation);
						float rotation2 = TerrainManager.Instance.TerrainDetailsRandom.RandfRange(0, settings.RandomizeTiltAngle);
						rotation2 = Mathf.DegToRad(rotation2);
						rotX = rotation2;//item.Model.RotateX(rotation2);
										 // objectToSpawn.relevantItem.RotateX(rotation2);
					}
					itm.Rotation = new Vector3(rotX, rotY, rotZ);
					itm.Scale = Vector3.One * TerrainManager.Instance.TerrainDetailsRandom.RandfRange(settings.MinSize, settings.MaxSize);
					itm.Hash = "";
					itm.ItemName = "Decor-" + itm.ModelAddress + i;

					//WorldItemSettings worldItem = TerrainDetailsManager.Instance.GetSpawnedObject(itm.ItemName);
					//WorldItem = TerrainManager.Instance.DetailsManager.SpawnObject(settings.Path);
					generated.Add(itm);
					//THIS FUNCTION SHOULD NOT TRY TO RETURN WORLDITEMS OR WORLDITEMS SHOULD NOT DIRECTLY BE THE 3D OBJECTS

				}
			}
			catch (Exception ex)
			{
				GD.Print(ex.StackTrace);
			}

			//GD.Print("[Item Action] Generating theoretical items " + generated.Count);
			return generated;

		}


		public static async Task<Map> LoadMapFromUnibyteAsync(string path)
		{
			//GD.Print("Loading file : " + path);
			int size = TerrainMeshSettings.numVertsPerLine;
			int chunkbytesize = (size * size) * 2;

			byte[] result = FileWriter.ReadISLToByte(documentspath + path + ".isl");
			//GD.Print("Reading : " + result.Length + " bytes");
			float[,] r = new float[size, size];
			Map map = new Map(r, Map.Origins.LoadedFromBinary);
			await Task.Run(() =>
			{

				int i = 0;
				for (int y = 0; y < size; y++)
				{
					for (int x = 0; x < size; x++)
					{
						byte[] rShort = { result[i], result[i + 1] };
						float position = ShortToHeightNormalized(BitConverter.ToInt16(rShort));
						r[x, y] = position;
						i = i + 2;
					}
				}

				byte[] details = FileWriter.ReadISLToByte(documentspath + path + "_D" + ".isl");

				List<WorldItem> worldItems = (List<WorldItem>)FileWriter.DeserializeFromBinary<List<WorldItem>>(details);
				//List<List<WorldItem>> worldItems = new List<List<WorldItemData>>();
				//worldItems.Add(worldItems);

				map.AddTerrainElements(worldItems);
			});
			return map;
		}


		public static short HeightToShortNormalized(float number)
		{
			float weighedHeight = Mathf.InverseLerp(TerrainManager.Instance.CurrentMapSettings.LowestPoint,
			TerrainManager.Instance.CurrentMapSettings.HighestPoint, number);

			return (short)Mathf.Lerp(-32768, 32767, weighedHeight);
		}
		public static float ShortToHeightNormalized(short number)
		{

			float weight = Mathf.InverseLerp(-32768,
			32767, number);
			float result = Mathf.Lerp(TerrainManager.Instance.CurrentMapSettings.LowestPoint, TerrainManager.Instance.CurrentMapSettings.HighestPoint,
			weight);
			return result;

		}

		public static bool MapExists(string path)
		{
			if (File.Exists(documentspath + path))
			{
				//GD.Print("Chunk exists in save");
				return true;
			}
			if (File.Exists(documentspath + path + "_D"))
			{
				//GD.Print("Chunk exists in save");
				return true;
			}
			return false;
		}

		public static float[,] GenerateHeightMapSimplex(int width, int height, TerrainPass settings, Vector2 sampleCentre, int passID = 1)
		{
			NoiseSettingsSimplexSmooth noiseSet = new NoiseSettingsSimplexSmooth();
			noiseSet.octaves = settings.Octaves;
			noiseSet.frequency = settings.Frequency;
			noiseSet.offset = settings.HorizontalScale;
			noiseSet.scale = Vector2.One * settings.VerticalScale;
			noiseSet.seed = TerrainManager.Instance.Seed + passID;
			float[,] values = Noise.GenerateNoiseMapSimplex(width, height, noiseSet, sampleCentre);
			/*for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					float v = values[x, y];

					if (v < settings.MinHeight || v > settings.MaxHeight)
					{
						values[x, y] = -201f; // sentinel = transparent
					}
				}
			}*/

			return values;
		}
	}


	//This is what composes a map. 
	public struct Map
	{
		public float[,] heightMap;

		public enum Origins { Generated, LoadedFromTexture, LoadedFromBinary }

		List<WorldItem> DecorElements = new List<WorldItem>();

		public Origins Origin;
		/*public Map()
		{
			
		}*/

		public Map(Origins origin)
		{



			Origin = origin;
		}

		public Map(float[,] _heightMapCoordinates, Origins origin)
		{

			heightMap = _heightMapCoordinates;


			Origin = origin;
		}

		public void AddTerrainElement(WorldItem element)
		{
			DecorElements.Add(element);
		}
		public void AddTerrainElements(List<WorldItem> elements)
		{
			DecorElements.AddRange(elements);
		}

		public void SetHeightmap(float[,] heightmap)
		{
			heightMap = heightmap;
		}

		public float[,] GetHeightmap()
		{
			return heightMap;
		}

		public List<WorldItem> GetTerrainElements()
		{
			return DecorElements;
		}

		public void SaveUnibyte(string name)
		{
			string documentspath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
				+ "/Islands/";

			int width = heightMap.GetLength(0);
			int height = heightMap.GetLength(1);

			//float[,] result = new float[width, height];
			byte[] buffer = new byte[(heightMap.GetLength(0) * heightMap.GetLength(1)) * 2]; //new byte[(valuesR.GetLength(0)*valuesR.GetLength(1))*2];
			int i = 0;

			for (int y = 0; y < width; y++)
			{
				for (int x = 0; x < height; x++)
				{
					byte[] newshort = new byte[2];
					short result = MapGenerator.HeightToShortNormalized(heightMap[x, y]);//MapGenerator.HeightToShortNormalized(ReturnValueR(x,y) + ReturnValueG(x,y));
																						 //GD.Print(result);
					newshort = BitConverter.GetBytes(result);
					Buffer.BlockCopy(newshort, 0, buffer, i, 2);
					i = i + 2;
				}
			}
			FileWriter.BinaryToISL(buffer, documentspath + name);

		}

		public void SaveMapDetails(string name)
		{
			string documentspath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
				+ "/Islands/";
			byte[] buffer = FileWriter.SerializeToBinary(DecorElements);
			GD.Print("Writing binaries " + buffer.Length);
			FileWriter.BinaryToISL(buffer, documentspath + name + "_D");
		}
	}

}