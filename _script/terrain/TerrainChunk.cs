using Godot;
using Bouncerock;
using System.Collections.Generic;
using Bouncerock.UI;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System;

namespace Bouncerock.Terrain
{
	public class TerrainChunk
	{

		//const float colliderGenerationDistanceThreshold = 5;
		//public event System.Action<TerrainChunk, bool> onVisibilityChanged;
		public Vector2 GridPosition;

		MeshInstance3D meshObject;
		public Vector2 sampleCentre;
		//Rect2 bounds;

		public Aabb Bounds;

		LODInfo[] detailLevels;
		LODMesh[] lodMeshes;
		//int colliderLODIndex;

		Map heightMap;
		bool heightMapReceived;
		public int previousLODIndex = -1;
		public int currentLODIndex = -1;
		bool hasSetCollider;

		bool hasSetMaterial;
		//float maxViewDst;

		HeightMapSettings heightMapSettings;
		//Vector3 viewer;

		StaticBody3D staticBody;
		CollisionShape3D collisionShape;

		//public List<MeshInstance3D> DecorHelpers = new List<MeshInstance3D>();

		public bool itemsLoaded = false;
		public List<List<WorldItem>> worldItems = new List<List<WorldItem>>();

		WorldSpaceUI helper;

		public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, LODInfo[] detailLevels, int currentLODIndex, Node parent)
		{
			GridPosition = coord;
			this.detailLevels = detailLevels;
			this.currentLODIndex = currentLODIndex;
			this.heightMapSettings = heightMapSettings;
			//GD.Print("Created : " + coord + ". At location: " + coord * TerrainMeshSettings.meshWorldSize);
			sampleCentre = coord * TerrainMeshSettings.meshWorldSize / TerrainMeshSettings.meshScale;
			Vector2 position = coord * TerrainMeshSettings.meshWorldSize;

			meshObject = new MeshInstance3D();
			meshObject.Name = "Chunk[x:" + coord.X + ",y:" + coord.Y + "]";
			parent.AddChild(meshObject);

			meshObject.Position = new Vector3(position.X, 0, position.Y);
			Bounds = new Aabb(position.X - (meshObject.Scale.X / 2), 0, position.Y - (meshObject.Scale.Y / 2), Vector3.One * TerrainMeshSettings.meshWorldSize);
			if (TerrainManager.Instance.ShowHelpers)
			{
				MeshInstance3D line = LineDrawer.DrawLine3D(meshObject.Position, meshObject.Position + Vector3.Up * 500, Colors.Black);
				meshObject.Name = "LineHelper";
				meshObject.AddChild(line);
			}
			lodMeshes = new LODMesh[detailLevels.Length];

			for (int i = 0; i < detailLevels.Length; i++)
			{
				lodMeshes[i] = new LODMesh(detailLevels[i].LodLevel);
				lodMeshes[i].updateCallback += OnLODMeshReceived;
				lodMeshes[i].updateCallback += UpdateCollisionMesh;
				lodMeshes[i].updateCallback += SetMaterial;
				lodMeshes[i].updateCallback += UpdateHelpers;
			}

		}

		/*void CreateBoundGizmo()
		{
			Color semitransparent = new Color(1,1,1,0.5f);
			MeshInstance3D GizmoHelper = LineDrawer.DrawAABB(Vector3.Zero, Bounds.Size, semitransparent);
			GizmoHelper.Name = "Gizmo Helper";
			meshObject.AddChild(GizmoHelper);
		}*/

		public Vector2 CoordToUnit()
		{
			return GridPosition / TerrainMeshSettings.chunkMeshSize;
		}

		public async void Load()
		{
			//Do I have a local save?
			Map map = new Map();
			if (TerrainManager.Instance.SaveTerrainToLocalDisk)
			{
				if (MapGenerator.MapExists("Chunk[x" + GridPosition.X + ",y" + GridPosition.Y + "].isl"))
				{
					map = await MapGenerator.LoadMapFromUnibyteAsync("Chunk[x" + GridPosition.X + ",y" + GridPosition.Y + "]");
					OnHeightMapReceived(map);
					//ThreadedDataRequester.RequestData(() => MapGenerator.LoadMapFromUnibyte("Chunk[x" + GridPosition.X + ",y" + GridPosition.Y+"]"), OnHeightMapReceived);
					return;
				}
			}
			map = await MapGenerator.GenerateMapAsync(sampleCentre);
			OnHeightMapReceived(map);
			//ThreadedDataRequester.RequestData(() => MapGenerator.GenerateMap (sampleCentre), OnHeightMapReceived);
		}

		//returns the height on the grid at given location. Location is express as local 

		public float GetHeightAtChunkMapLocation(Vector2 location)
		{
			try
			{
				// Clamp the x and y indices to ensure they are within bounds.
				int xClamped = Mathf.RoundToInt(location.X);
				int yClamped = Mathf.RoundToInt(location.Y);

				// Access the height map with clamped indices.
				return heightMap.heightMap[xClamped, yClamped];
			}
			catch (Exception)
			{
				//GD.Print("out of bounds" + location.X +"/"+location.Y);
				// Return -201 if any exception occurs (e.g., out of bounds).
				return -201;
			}
		}

		public float GetInclinationAtChunkMapLocation(Vector2 location)
		{
			int xClamped = Mathf.CeilToInt(location.X);
			int yClamped = Mathf.CeilToInt(location.Y);
			float height = heightMap.heightMap[xClamped, yClamped];

			Vector3 v1 = new Vector3(xClamped, height, yClamped);
			Vector3 v2 = new Vector3(xClamped, heightMap.heightMap[xClamped, yClamped + 1], yClamped + 1);
			Vector3 v3 = new Vector3(xClamped + 1, heightMap.heightMap[xClamped + 1, yClamped + 1], yClamped + 1);
			Vector3 v4 = new Vector3(xClamped + 1, heightMap.heightMap[xClamped + 1, yClamped], yClamped);

			//float inclination = MathExt.CalculateInclination(verticle1,verticle2,verticle3,verticle4);

			Vector3 add = v2 - v1;
			Vector3 normal = add.Cross(v3 - v1).Normalized();

			// Compute the angle between the normal and the world up vector
			float inclination = Mathf.Acos(normal.Dot(Vector3.Up)) * 57.29578f;


			return inclination;
		}
		public float[,] GetHeightmap()
		{
			if (heightMap.heightMap != null) { return heightMap.heightMap; }
			return null;
		}
		public List<List<WorldItemData>> GetItems()
		{
			if (heightMap.DecorElements != null) { return heightMap.DecorElements; }
			return null;
		}



		void OnHeightMapReceived(object heightMapObject)
		{
			this.heightMap = (Map)heightMapObject;
			if (heightMap.Origin == Map.Origins.Generated && TerrainManager.Instance.SaveTerrainToLocalDisk)
			{
				heightMap.SaveUnibyte("Chunk[x" + GridPosition.X + ",y" + GridPosition.Y + "]");
				heightMap.SaveMapDetails("Chunk[x" + GridPosition.X + ",y" + GridPosition.Y + "]");
			}
			heightMapReceived = true;
			OnLODChanged();
			LoadTerrainElements();
		}



		async void LoadTerrainElements()
		{
			//GD.Print("Loading terrain elements, with " + heightMap.DecorElements.Count + " elements");
			Node3D parentNode = new Node3D();
			parentNode.Name = "DecorObjects";
			meshObject.CallDeferred("add_child", parentNode);
			if (itemsLoaded) { return; }
			int i = 0;
			foreach (List<WorldItemData> worldItem in heightMap.DecorElements)
			{
				//GD.Print("Loading "+worldItem.Count + " elements");
				List<WorldItem> items = new List<WorldItem>();
				foreach (WorldItemData itm in worldItem)
				{

					Vector2 inGridLocation = new Vector2(25 - itm.GridLocation.X, 25 - itm.GridLocation.Y);

					Vector3 location = new Vector3(-1 * inGridLocation.X, GetHeightAtChunkMapLocation(itm.GridLocation), inGridLocation.Y);
					
					itm.SetElevation(location.Y);
					//if (location.Y < 0) { continue; }
					//GD.Print("itm.name" + itm.name);
					var naturalObject = TerrainDetailsManager.Instance.GetSpawnedObject(itm.name);
					;
					if (naturalObject == null)
					{ //GD.Print("Item " + itm.name + " couldn't be found");
						continue;
					}
					if (naturalObject is NaturalObject)
					{
						NaturalObject naturelOb = (NaturalObject)naturalObject;
						if (location.Y < naturelOb.MinimumSpawnAltitude || location.Y > naturelOb.MaximumSpawnAltitude)
						{
							continue;
						}
					}
					else
					{
						if (location.Y < 0) { continue; }
					}
					WorldItem item = await TerrainManager.Instance.DetailsManager.SpawnAndInitialize(naturalObject);

					//GD.Print("In grid "+itm.GridLocation);
					parentNode.CallDeferred("add_child", item);
					item.Position = location;
					//RandomNumberGenerator rnd = new RandomNumberGenerator();
					//item.Scale = Vector3.One*rnd.RandfRange(0.1f,1f);

					item.Name = "Decor-" + itm.name + i;
					items.Add(item);


					i++;
				}
				worldItems.Add(items);
			}
			itemsLoaded = true;
		}

		Vector2 viewerPosition
		{
			get
			{
				if (GameManager.Instance.MainCamera == null) { return Vector2.Zero; }
				return new Vector2(GameManager.Instance.MainCamera.GlobalPosition.X, GameManager.Instance.MainCamera.GlobalPosition.Z);
			}
		}

		Vector3 viewerPosition3
		{
			get
			{
				if (GameManager.Instance.MainCamera == null) { return Vector3.Zero; }
				return new Vector3(GameManager.Instance.MainCamera.GlobalPosition.X, 0, GameManager.Instance.MainCamera.GlobalPosition.Z);
			}
		}

		public void OnLODMeshReceived()
		{
			//GD.Print(GridPosition + " - Received mesh for LOD " + currentLODIndex);
			LODMesh lodMesh = lodMeshes[currentLODIndex];
			if (lodMesh.hasMesh)
			{
				previousLODIndex = currentLODIndex;
				meshObject.Mesh = lodMesh.mesh;
			}
		}
		public async void OnLODChanged()
		{
			if (!heightMapReceived)
			{
				//GD.Print("LOD Changed, but did not receive heightmap");
			}
			if (heightMapReceived)
			{
				if (currentLODIndex != previousLODIndex)
				{
					//GD.Print(GridPosition + " - Lod changed: from " + previousLODIndex + " to " + currentLODIndex);
					LODMesh lodMesh = lodMeshes[currentLODIndex];
					if (lodMesh.hasMesh)
					{
						previousLODIndex = currentLODIndex;
						meshObject.Mesh = lodMesh.mesh;
					}
					else if (!lodMesh.hasRequestedMesh)
					{
						previousLODIndex = currentLODIndex;
						await lodMesh.RequestMesh(heightMap);
					}
				}
				UpdateCollisionMesh();
			}
		}

		public void UpdateHelpers()
		{
			/*string text = meshObject.Name + " -  LOD: " + currentLODIndex + "\nCenter: " + sampleCentre + " " + IsVisible();
			
			if (helper == null)
			{
				helper = Debug.SetTextHelper(text, Vector3.Zero, meshObject);
				helper.MaxViewDistance = 1000;
			}
			if (helper != null)
			{
				helper.SetText(text);
			}*/
		}

		public void Destroy()
		{
			meshObject.QueueFree();
		}

		public void UpdateCollisionMesh()
		{
			//if (TerrainManager.Instance.detailLevels[currentLODIndex].HasCollider) {return;}
			if (TerrainManager.Instance.detailLevels[currentLODIndex].HasCollider)
			{
				if (hasSetCollider)
				{
					collisionShape.Disabled = false;
					collisionShape.Visible = true;
				}
				if (!hasSetCollider)
				{

					if (lodMeshes[currentLODIndex].hasMesh)
					{


						staticBody = new StaticBody3D();
						staticBody.Name = "Collision - " + meshObject.Name;
						meshObject.CallDeferred("add_child", staticBody);
						collisionShape = new CollisionShape3D();
						staticBody.CallDeferred("add_child", collisionShape);

						collisionShape.Shape = meshObject.Mesh.CreateTrimeshShape();

						hasSetCollider = true;
					}
				}

			}
			if (!TerrainManager.Instance.detailLevels[currentLODIndex].HasCollider && hasSetCollider && collisionShape != null)
			{

				collisionShape.Visible = false;
				collisionShape.Disabled = true;
			}
		}

		



		void SetMaterial()
		{
			if (!hasSetMaterial)
			{
				//Material mat = GD.Load<Material>("_material/test_standard_material_3d.tres");
				Material mat = TerrainManager.Instance.TerrainMaterial;
				if (TerrainManager.Instance.UseDebugMaterial)
				{
					mat = TerrainManager.Instance.DebugTerrainMaterial;
				}
				//Material mat = GD.Load<Material>("_material/testmat.tres");
				meshObject.SetSurfaceOverrideMaterial(0, mat);
				hasSetMaterial = true;
			}
		}

		public void SetVisible(bool visible)
		{
			meshObject.Visible = visible;
		}

		public bool IsVisible()
		{
			return meshObject.Visible;
		}

	}

	class LODMesh
	{

		public Mesh mesh;
		public bool hasRequestedMesh;
		public bool hasMesh;
		int lod;
		public event System.Action updateCallback;

		public LODMesh(int lod)
		{
			this.lod = lod;
		}


		async Task OnMeshDataReceived(MeshData meshDataObject)
		{
			//GD.Print("Mesh received");
			mesh = await ((MeshData)meshDataObject).CreateMesh();

			hasMesh = true;
			updateCallback();
		}



		public async Task RequestMesh(Map heightMap)
		{
			//GD.Print("Requesting Mesh");
			hasRequestedMesh = true;
			MeshData mesh = await MeshGenerator.GenerateTerrainMeshAsync(heightMap, lod);
			await OnMeshDataReceived(mesh);
			//ThreadedDataRequester.RequestData (() => MeshGenerator.GenerateTerrainMesh (heightMap, lod), OnMeshDataReceived);
		}

	}
}