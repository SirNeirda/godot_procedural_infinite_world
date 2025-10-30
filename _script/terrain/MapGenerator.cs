
using Godot;
using System.IO;
using Bouncerock;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bouncerock.Procedural;

namespace Bouncerock.Terrain
{
	public static class MapGenerator
	{



		static string documentspath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
				+ "/Islands/";
		//This is where new chunks are generated and assembled. For now we just use a basic simplex generator. This code should be updated later to include more complex generation.

public static async Task<Map> GenerateMapAsync(Vector2 sampleCentre)
{
    Vector2 offset = new Vector2(
        -(TerrainMeshSettings.numVertsPerLine / 2) + sampleCentre.X,
        (TerrainMeshSettings.numVertsPerLine / 2) - sampleCentre.Y);

    Map map = new Map();

    await Task.Run(async () =>
    {
        try
        {
            // --- Base heightmap ---
            TerrainPass basePass = TerrainManager.Instance.CurrentMapSettings.Passes[0];
            float[,] heightMap = GenerateHeightMapSimplex(
                TerrainMeshSettings.numVertsPerLine,
                TerrainMeshSettings.numVertsPerLine,
                basePass,
                offset);

            ApplyContrast(heightMap, basePass.Contrast);

            // --- Additive passes ---
            for (int i = 1; i < TerrainManager.Instance.CurrentMapSettings.Passes.Count; i++)
            {
                TerrainPass pass = TerrainManager.Instance.CurrentMapSettings.Passes[i];
                float[,] heightMapTemp = GenerateHeightMapSimplex(
                    TerrainMeshSettings.numVertsPerLine,
                    TerrainMeshSettings.numVertsPerLine,
                    pass,
                    offset);

                ApplyContrast(heightMapTemp, pass.Contrast);

                for (int y = 0; y < TerrainMeshSettings.numVertsPerLine; y++)
                {
                    for (int x = 0; x < TerrainMeshSettings.numVertsPerLine; x++)
                    {
                        heightMap[x, y] += heightMapTemp[x, y];
                        heightMap[x, y] = Mathf.Clamp(heightMap[x, y], pass.MinHeight, pass.MaxHeight);
                    }
                }
            }

            // (Pathways, lakes, world items as before...)
            // --- Finalize map ---
            map = new Map(heightMap, Map.Origins.Generated);

            List<List<WorldItemData>> _worldItems = new List<List<WorldItemData>>();
            foreach (NaturalObject naturalObject in TerrainManager.Instance.CurrentMapSettings.NaturalObjects)
            {
                if (naturalObject.Concentration > 1)
                {
                    List<WorldItemData> items = await GenerateStaticElements(naturalObject);
                    if (items.Count > 0) _worldItems.Add(items);
                }
                else
                {
                    WorldItemData item = await DetermineItemPresence(naturalObject);
                    if (item.name != "") _worldItems.Add(new List<WorldItemData>() { item });
                }
            }

            foreach (GameplayObject gameplayObject in TerrainManager.Instance.CurrentMapSettings.GameplayObjects)
            {
                if (gameplayObject.Concentration > 1)
                {
                    List<WorldItemData> items = await GenerateStaticElements(gameplayObject);
                    if (items.Count > 0) _worldItems.Add(items);
                }
                else
                {
                    WorldItemData item = await DetermineItemPresence(gameplayObject);
                    if (item.GridLocation != Vector2.Zero) _worldItems.Add(new List<WorldItemData>() { item });
                }
            }

            foreach (List<WorldItemData> worldItem in _worldItems)
            {
                map.AddTerrainElements(worldItem);
            }
        }
        catch (Exception ex)
        {
            GD.Print("Error in Map Generation: " + ex.StackTrace);
        }
    });

    return map;
}

		/*public static async Task<Map> GenerateMapAsync_deprecated(Vector2 sampleCentre)
		{
			Vector2 offset = new Vector2(-(TerrainMeshSettings.numVertsPerLine / 2) + sampleCentre.X,
										 (TerrainMeshSettings.numVertsPerLine / 2) - sampleCentre.Y);

			NoiseSettingsSimplexSmooth noiseSetting = new NoiseSettingsSimplexSmooth();
			Map map = new Map();

			await Task.Run(async () =>
			{
				try
				{
					// Base heightmap
					noiseSetting.scale = TerrainManager.Instance.CurrentMapSettings.Passes[0].VerticalScale;
					noiseSetting.octaves = TerrainManager.Instance.CurrentMapSettings.Passes[0].Octaves;
					noiseSetting.frequency = TerrainManager.Instance.CurrentMapSettings.Passes[0].Frequency;

					noiseSetting.offset = offset;

					float[,] heightMap = GenerateHeightMapSimplex(
						/*TerrainMeshSettings.numVertsPerLine, TerrainMeshSettings.numVertsPerLine,
						noiseSetting, sampleCentre);

					ApplyContrast(heightMap, TerrainManager.Instance.CurrentMapSettings.Passes[0].Contrast);

					// Apply all passes
					for (int y = 0; y < TerrainMeshSettings.numVertsPerLine; y++)
					{
						for (int x = 0; x < TerrainMeshSettings.numVertsPerLine; x++)
						{
							heightMap[x, y] += Mathf.Clamp(
								heightMap[x, y] * TerrainManager.Instance.CurrentMapSettings.Passes[0].HorizontalScale,
								TerrainManager.Instance.CurrentMapSettings.LowestPoint,
								TerrainManager.Instance.CurrentMapSettings.HighestPoint);
						}
					}

					for (int i = 1; i < TerrainManager.Instance.CurrentMapSettings.Passes.Count; i++)
					{
						noiseSetting.scale = TerrainManager.Instance.CurrentMapSettings.Passes[i].VerticalScale;
						noiseSetting.octaves = TerrainManager.Instance.CurrentMapSettings.Passes[i].Octaves;
						noiseSetting.frequency = TerrainManager.Instance.CurrentMapSettings.Passes[i].Frequency;
						noiseSetting.offset = offset;

						float[,] heightMapTemp = GenerateHeightMapSimplex(
							TerrainMeshSettings.numVertsPerLine, TerrainMeshSettings.numVertsPerLine,
							noiseSetting, sampleCentre);

						ApplyContrast(heightMapTemp, TerrainManager.Instance.CurrentMapSettings.Passes[i].Contrast);

						for (int y = 0; y < TerrainMeshSettings.numVertsPerLine; y++)
						{
							for (int x = 0; x < TerrainMeshSettings.numVertsPerLine; x++)
							{
								float value = heightMapTemp[x, y] * TerrainManager.Instance.CurrentMapSettings.Passes[i].HorizontalScale;

								heightMap[x, y] += Mathf.Clamp(value,
									TerrainManager.Instance.CurrentMapSettings.LowestPoint,
									TerrainManager.Instance.CurrentMapSettings.HighestPoint);
							}
						}
					}

					// Pathways
					PathwaySettings pathwaySettings = TerrainManager.Instance.CurrentMapSettings.Pathway;
					float[,] pathwayMask = GeneratePathwayMaskSimplex(
						TerrainMeshSettings.numVertsPerLine, TerrainMeshSettings.numVertsPerLine,
						noiseSetting, sampleCentre, pathwaySettings);

					ApplyPathwayFlattening(heightMap, pathwayMask, pathwaySettings);

					// Lakes
					ApplyLakes(heightMap, 0.25f);

					// Place natural + gameplay objects
					List<List<WorldItemData>> _worldItems = new List<List<WorldItemData>>();

					foreach (NaturalObject naturalObject in TerrainManager.Instance.CurrentMapSettings.NaturalObjects)
					{
						if (naturalObject.Concentration > 1)
						{
							List<WorldItemData> items = await GenerateStaticElements(naturalObject);
							if (items.Count > 0) _worldItems.Add(items);
						}
						else
						{
							WorldItemData item = await DetermineItemPresence(naturalObject);
							if (item.name != "") _worldItems.Add(new List<WorldItemData>() { item });
						}
					}

					foreach (GameplayObject gameplatObject in TerrainManager.Instance.CurrentMapSettings.GameplayObjects)
					{
						if (gameplatObject.Concentration > 1)
						{
							List<WorldItemData> items = await GenerateStaticElements(gameplatObject);
							if (items.Count > 0) _worldItems.Add(items);
						}
						else
						{
							WorldItemData item = await DetermineItemPresence(gameplatObject);
							if (item.GridLocation != Vector2.Zero) _worldItems.Add(new List<WorldItemData>() { item });
						}
					}

					// Finalize map
					map = new Map(heightMap, Map.Origins.Generated);
					foreach (List<WorldItemData> worldItem in _worldItems)
					{
						map.AddTerrainElements(worldItem);
					}
				}
				catch (Exception ex)
				{
					GD.Print("Error in Map Generation: " + ex.StackTrace);
				}
			});

			return map;
		}*/




		private static void ApplyContrast(float[,] heightMap, float contrast, float mid = 0)
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
		}



		private static void ApplyLakes(float[,] heightMap, float waterHeight = 0.25f)
		{
			for (int y = 0; y < heightMap.GetLength(1); y++)
			{
				for (int x = 0; x < heightMap.GetLength(0); x++)
				{
					if (heightMap[x, y] < waterHeight)
						heightMap[x, y] = waterHeight;
				}
			}
		}


		static async Task<WorldItemData> DetermineItemPresence(SpawnedObject naturalObject)
		{
			WorldItemData item = new WorldItemData();
			RandomNumberGenerator rnd = new RandomNumberGenerator();
			rnd.Seed = (ulong)DateTime.Now.ToBinary();
			float chance = rnd.RandfRange(0, 1);
			//GD.Print("Item present " + naturalObject.ObjectName + " chance " + chance);
			if (naturalObject.Concentration >= chance)
			{
				Vector2 location = new Vector2();
				location.X = rnd.RandfRange(0, TerrainMeshSettings.numVertsPerLine);
				location.Y = rnd.RandfRange(0, TerrainMeshSettings.numVertsPerLine);
				item.name = naturalObject.ObjectName;
				item.GridLocation = location;
				return item;
			}
			return item;
		}

		static async Task<List<WorldItemData>> GenerateStaticElements(SpawnedObject naturalObject)
		{
			//List<Vector2> locations = await PoissonDiscSampling.Test(Vector2.One * (TerrainMeshSettings.numVertsPerLine-3), Vector2.Zero, 10);
			int seed = (int)DateTime.Now.ToBinary();
			//int minSpacing = Math.Clamp(70-(int)naturalObject.Concentration, 5,30);
			List<Vector2> locations = await PoissonDiscSampling.GeneratePoints(Vector2.Zero, 10, Vector2.One * TerrainMeshSettings.numVertsPerLine, (int)naturalObject.Concentration, seed);
			//GD.Print("Poisson disc loc: " + locations.Count + " elements ");

			List<WorldItemData> generated = new List<WorldItemData>();

			foreach (Vector2 location in locations)
			{
				WorldItemData itm = new WorldItemData();
				WorldItemSettings settings = new WorldItemSettings();

				itm.GridLocation = location;
				//GD.Print("INGRIDLOC " + itm.GridLocation);
				itm.name = naturalObject.ObjectName;
				if (settings.MinSize != settings.MaxSize)
				{
					RandomNumberGenerator rnd = new RandomNumberGenerator();
					rnd.Seed = (ulong)location.X;
					itm.Size = Vector3.One * rnd.RandfRange(settings.MinSize, settings.MaxSize);
				}


				//WorldItem = TerrainManager.Instance.DetailsManager.SpawnObject(settings.Path);
				generated.Add(itm);

			}

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

				List<WorldItemData> list = (List<WorldItemData>)FileWriter.DeserializeFromBinary<List<WorldItemData>>(details);
				List<List<WorldItemData>> worldItems = new List<List<WorldItemData>>();
				worldItems.Add(list);

				map.DecorElements = worldItems;
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

		public static float[,] GenerateHeightMapSimplex(int width, int height, TerrainPass settings, Vector2 sampleCentre)
		{
			NoiseSettingsSimplexSmooth noiseSet = new NoiseSettingsSimplexSmooth();
			noiseSet.octaves = settings.Octaves;
			noiseSet.frequency = settings.Frequency;
			noiseSet.offset = settings.HorizontalScale;
			noiseSet.scale = Vector2.One*settings.VerticalScale;

			float[,] values = Noise.GenerateNoiseMapSimplex(width, height, noiseSet, sampleCentre);

			return values;
		}

		/*public static float[,] GeneratePathwayMaskSimplex(int mapWidth, int mapHeight, NoiseSettingsSimplexSmooth settings, Vector2 sampleCentre, PathwaySettings pathwaySettings)
		{
			float[,] pathwayMask = new float[mapWidth, mapHeight];

			// Generate Simplex noise map for pathways
			float[,] noiseMap = Noise.GenerateNoiseMapSimplex(mapWidth, mapHeight, settings, sampleCentre);

			for (int y = 0; y < mapHeight; y++)
			{
				for (int x = 0; x < mapWidth; x++)
				{
					// Pathways are formed based on a noise threshold, creating narrow paths
					if (Mathf.Abs(noiseMap[x, y] - 0.5f) < pathwaySettings.Threshold)
					{
						pathwayMask[x, y] = 1f; // Mark this area as part of the pathway
					}
					else
					{
						pathwayMask[x, y] = 0f; // No pathway
					}
				}
			}

			return pathwayMask;
		}*/
	}


	//This is what composes a map. 
	public struct Map
	{
		public readonly float[,] heightMap;

		public enum Origins { Generated, LoadedFromTexture, LoadedFromBinary }

		public List<List<WorldItemData>> DecorElements = new List<List<WorldItemData>>();

		public Origins Origin;

		public Map(float[,] _heightMapCoordinates, Origins origin)
		{

			heightMap = _heightMapCoordinates;


			Origin = origin;
		}

		public void AddTerrainElements(List<WorldItemData> elements)
		{
			DecorElements.Add(elements);
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