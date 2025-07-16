using Godot;
using Bouncerock.Events;
using System.Collections.Generic;
using System;

namespace Bouncerock.Terrain
{
	public partial class TerrainManager : Node,
											BouncerockEventListener<EvtCameraChanged>
	{
		[Export]
		public Camera3D Viewer;
		//public Material mapMaterial;

		[Export]
		public bool UpdateTerrain = true;
		[Export]
		public bool SaveTerrainToLocalDisk = true;
		[Export]
		public int ViewingDistance = 3;
		[Export]
		public int Seed = 1234;
		[Export]
		public bool UseRandomSeed = true;
		[Export]
		public Material TerrainMaterial;
		
		[Export]
		public float UpdateFrequency = 1;

		[Export]
		public bool ShowHelpers = false;


		public RandomNumberGenerator TerrainDetailsRandom = new RandomNumberGenerator();
		//const float viewerMoveThresholdForChunkUpdate = 15f;
		//const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

		public static TerrainManager Instance;

		public TerrainDetailsManager DetailsManager;
		//public int colliderLODIndex;
		public LODInfo[] detailLevels;

		//public MeshSettings meshSettings;
		public HeightMapSettings heightMapSettings;
		//public TextureData textureSettings;

		

		public MapGenerationSettings CurrentMapSettings;

		
		float updateTimer = 0;

		Vector2 viewerPosition;
		Vector2 viewerPositionOld;

		float meshWorldSize;
		//int chunksVisibleInViewDst;

		Dictionary<Vector2, TerrainChunk> chunksDictionary = new Dictionary<Vector2, TerrainChunk>();
		//List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

		TerrainChunk currentChunk;

		Vector2 currentHalfChunkPosition = Vector2.Zero;

		public bool Initialized = false;

		//bool set = false;

		public override void _Ready()
		{
			this.BouncerockEventStartListening<EvtCameraChanged>();
			//ulong seed = (ulong)Seed;
			if (UseRandomSeed)
			{
				Seed = (int)DateTime.UtcNow.Ticks;
			}
			TerrainDetailsRandom.Seed = (ulong)Seed;
			LoadNewTerrain();
		}

		public void LoadNewTerrain()
		{
			Initialized = false;
			GD.Print("Initializing map");
			CurrentMapSettings = new MapGenerationSettings();
			CurrentMapSettings.DefaultValues();
			Instance = this;
			if (GameManager.Instance.MainCamera != null)
			{
				Viewer = GameManager.Instance.MainCamera;
			}
			SetupLOD();
			//SetupMeshSettings();
			DetailsManager = GetNode<TerrainDetailsManager>("TerrainDetailsManager");
			meshWorldSize = TerrainMeshSettings.meshWorldSize;
			Initialized = true;
		}

		void SetupLOD()
		{
			detailLevels = new LODInfo[6];

			//The higest LOD level, the chunk where the player is
			LODInfo inf0 = new LODInfo();
			inf0.LodLevel = 0; 
			inf0.AdjacencyLevel = 0; 
			inf0.HasCollider = true;
			//inf0.lod = 0; inf0.visibleDstThreshold = 100;

			//Also the highest LOD level, the chunks adjacent to the part of the chunk the player is closest to
			LODInfo inf1 = new LODInfo();
			inf1.LodLevel = 0; 
			inf1.AdjacencyLevel = 1; 
			inf1.HasCollider = true;
			//inf1.lod = 4; inf1.visibleDstThreshold = 200;

			//First level of adjacency, 
			LODInfo inf2 = new LODInfo();
			//inf2.lod = 5; inf2.visibleDstThreshold = 400;
			inf2.LodLevel = 2; 
			inf2.AdjacencyLevel = 2; 
			inf2.HasCollider = false;

			//Second level of adjacency
			LODInfo inf3 = new LODInfo();
			inf3.LodLevel = 3; 
			inf3.AdjacencyLevel = 3; 
			inf3.HasCollider = false;

			LODInfo inf4 = new LODInfo();
			inf4.LodLevel = 4; 
			inf4.AdjacencyLevel = 4; 
			inf4.HasCollider = false;

			LODInfo inf5 = new LODInfo();
			inf4.LodLevel = 4; 
			inf4.AdjacencyLevel = 5; 
			inf4.HasCollider = false;

			detailLevels[0] = inf0;
			detailLevels[1] = inf1;
			detailLevels[2] = inf2;
			detailLevels[3] = inf3;
			detailLevels[4] = inf4;
			//detailLevels[5] = inf5;
			//detailLevels[4] = inf4;
		}
		void SetupMeshSettings()
		{
			//meshSettings = new MeshSettings();
		}
		public float[,] GetHeightmapForChunk(Vector2 chunkLoc)
		{
			if (chunksDictionary.ContainsKey(chunkLoc)) 
			{
				return chunksDictionary[chunkLoc].GetHeightmap();
			}
			return null;
		}

		public List<List<WorldItemData>> GetItemsForChunk(Vector2 chunkLoc)
		{
			if (chunksDictionary.ContainsKey(chunkLoc)) 
			{
				return chunksDictionary[chunkLoc].GetItems();
			}
			return null;
		}

		//This returns the height at a specific world space coordinate
		//Returns -201 if a value couldn't be determined, since 200 is the lowest elevation possible
		public float GetTerrainHeightAtGlobalCoordinate(Vector2 location)
		{
			int chunkCoordX = Mathf.RoundToInt (location.X / meshWorldSize);
			int chunkCoordY = Mathf.RoundToInt (location.Y / meshWorldSize);

			Vector2 chunkLoc = new Vector2(chunkCoordX, chunkCoordY);
			if (chunksDictionary.ContainsKey(chunkLoc)) 
				{
					
					Vector2 inGridLocation = chunksDictionary[chunkLoc].sampleCentre - location;
					inGridLocation.X = TerrainMeshSettings.numVertsPerLine - (inGridLocation.X + TerrainMeshSettings.numVertsPerLine/2 -1) -3;
					inGridLocation.Y = inGridLocation.Y + TerrainMeshSettings.numVertsPerLine/2 -1;
					
					return chunksDictionary[chunkLoc].GetHeightAtChunkMapLocation(inGridLocation);

					//inGridLocation goes from -25 to +25 since the size of a chunk is 50
					//Interrogate the relevant chunk
				}
			return -201;
		}

		public float GetTerrainInclinationAtGlobalCoordinate(Vector2 location)
		{
			int chunkCoordX = Mathf.RoundToInt (location.X / meshWorldSize);
			int chunkCoordY = Mathf.RoundToInt (location.Y / meshWorldSize);

			Vector2 chunkLoc = new Vector2(chunkCoordX, chunkCoordY);

			if (chunksDictionary.ContainsKey(chunkLoc)) 
				{
					
					Vector2 inGridLocation = chunksDictionary[chunkLoc].sampleCentre - location;
					//GD.Print("Current chunk center: " + inGridLocation);
					//GD.Print("In Chunk grid location : " + inGridLocation);
					inGridLocation.X = TerrainMeshSettings.numVertsPerLine - (inGridLocation.X + TerrainMeshSettings.numVertsPerLine/2 -1) -3;
					inGridLocation.Y = inGridLocation.Y + TerrainMeshSettings.numVertsPerLine/2 -1;//TerrainMeshSettings.numVertsPerLine - (inGridLocation.Y + TerrainMeshSettings.numVertsPerLine/2 -1) -3 ;

					return chunksDictionary[chunkLoc].GetInclinationAtChunkMapLocation(inGridLocation);

				}
			return -201;
		}
		public Vector2 WorldspaceToChunkCoordinate(Vector2 coordinates)
		{
			int chunkCoordX = Mathf.RoundToInt (coordinates.X / meshWorldSize);
			int chunkCoordY = Mathf.RoundToInt (coordinates.Y / meshWorldSize);

			return new Vector2(chunkCoordX, chunkCoordY);
		}

		//This returns the coordinate stated in world space inside the local heightmap grid.
		public Vector2 WorldspaceToChunkMapLocation(Vector2 coordinates)
		{
			int chunkCoordX = Mathf.RoundToInt (coordinates.X / meshWorldSize);
			int chunkCoordY = Mathf.RoundToInt (coordinates.Y / meshWorldSize);
			Vector2 chunkLoc = new Vector2(chunkCoordX, chunkCoordY);

			if (chunksDictionary.ContainsKey(chunkLoc)) 
				{
					
					Vector2 inGridLocation = chunksDictionary[chunkLoc].sampleCentre - coordinates;
					inGridLocation.X = TerrainMeshSettings.numVertsPerLine - (inGridLocation.X + TerrainMeshSettings.numVertsPerLine/2 -1) -3;
					inGridLocation.Y = inGridLocation.Y + TerrainMeshSettings.numVertsPerLine/2 -1;

					return inGridLocation;

				}
			return(Vector2.One *-1);
		}

		

		public Vector2 CameraInChunk()
		{
			int currentChunkCoordX = Mathf.RoundToInt (viewerPosition.X / meshWorldSize);
			int currentChunkCoordY = Mathf.RoundToInt (viewerPosition.Y / meshWorldSize);
			return new Vector2(currentChunkCoordX, currentChunkCoordY); 
		}

		public override void _Process(double time) 
		{
			if (Viewer == null) {;return;}
			if (Initialized == false) {;return;}
			viewerPosition = new Vector2 (Viewer.GlobalPosition.X, Viewer.GlobalPosition.Z);
			
			updateTimer = updateTimer + (float)time;
			if (updateTimer > UpdateFrequency)
			{
				UpdateChunks();
				updateTimer = 0;
			}
		}

		void UpdateChunks()
		{
			// (set) {return;}
			GD.Print("Updating chunks");
			Vector2 newChunkPosition = CameraInChunk();
			if (currentChunk == null) 
				{
					//GD.Print("First time loading.");
					//First time loading. 
					TerrainChunk newChunk = new TerrainChunk (newChunkPosition,heightMapSettings, detailLevels, 0, this);
					chunksDictionary.Add (newChunkPosition, newChunk);
					//newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
					newChunk.Load ();
					currentChunk = newChunk;
					return;
				}

			if (currentChunk.GridPosition != newChunkPosition)
			{
				//GD.Print("Entering new chunk : " + newChunkPosition.X + "/" + newChunkPosition.Y );
				if (!chunksDictionary.ContainsKey(newChunkPosition)) {GD.Print("Wierd: Chunk" + newChunkPosition.X + "/" + newChunkPosition.Y + "is not in dictionnary");}
				currentChunk = chunksDictionary[newChunkPosition];
				currentHalfChunkPosition = MathExt.GetCurrentHalfBoundPosition(viewerPosition, currentChunk.Bounds);
			}
			
			Vector2 newHalfChunk = MathExt.GetCurrentHalfBoundPosition(viewerPosition, currentChunk.Bounds);
			//GD.Print("Calculated the bound " + currentChunk.Bounds.Position + " The result was " + newHalfChunk + "Current chunks: " + chunksDictionary.Count);
			string textt = "Half-chunk : "+ string.Format("{0:0. #}", newHalfChunk.X) + "/" + string.Format("{0:0. #}", newHalfChunk.Y);
			Debug.SetStaticBug("Chunk: " + currentChunk.GridPosition + ". Position: " + string.Format("{0:0. #}", viewerPosition.X)+ "/" + string.Format("{0:0. #}", viewerPosition.Y) + " - " + textt);
			if (newHalfChunk != currentHalfChunkPosition)
			{
				//GD.Print("Entering new half chunk, not doing anything else yet.");
				//We have changed the position in the chunk enough to get close to other chunks. We need to recompute all the chunks that need to be
				//updated
				Vector2[] FirstLodChunks = new Vector2[4];
				
				float lowestX = newHalfChunk.X==-1?currentChunk.GridPosition.X-1:currentChunk.GridPosition.X;
				float lowestY = newHalfChunk.Y==-1?currentChunk.GridPosition.Y-1:currentChunk.GridPosition.Y;


				FirstLodChunks[0] = new Vector2(lowestX,lowestY); //Lowest
				FirstLodChunks[1] = new Vector2(lowestX,lowestY+1);
				FirstLodChunks[2] = new Vector2(lowestX+1,lowestY+1);//Highest
				FirstLodChunks[3] = new Vector2(lowestX+1,lowestY);
				
				List<Vector2>[] chunksLayered = ComputeNewChunksAddresses(FirstLodChunks);

			
				HashSet<Vector2> reviewedChunks = new HashSet<Vector2> ();
				for (int i = 0; i < chunksLayered.Length; i++)
					{
						//Loop through each lod coordinate in the fresh list to see if we need to change the chunk
						foreach (Vector2 coord in chunksLayered[i])
						{
							if (chunksDictionary.ContainsKey(coord) && chunksDictionary[coord].currentLODIndex != i) 
							{
								//LOD has changed, initiate LOD change
								chunksDictionary[coord].currentLODIndex = i;
								chunksDictionary[coord].OnLODChanged ();
								chunksDictionary[coord].UpdateHelpers ();
							}
							if (!chunksDictionary.ContainsKey(coord)) 
							{
								//New chunk
								TerrainChunk newChunk = new TerrainChunk (coord,heightMapSettings, detailLevels, i, this);
								chunksDictionary.Add (coord, newChunk);
								//newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
								newChunk.Load ();
							}
							
							reviewedChunks.Add(coord);
						}
					}
				//GD.Print("We reviewedreviewed "+ reviewedChunks.Count);
				//Last, clean up unused chunks
				foreach(Vector2 coord in chunksDictionary.Keys)
					{
					 if (!reviewedChunks.Contains(coord))
						{
							//Chunk isn't visible anymore, dispose
							chunksDictionary[coord].Destroy();
							chunksDictionary.Remove(coord);
						}
					}
				


			}
			else 
			{
				//GD.Print("No mode detected for half chunk position");
			}
			currentHalfChunkPosition = newHalfChunk;
		}

		

		//This will compute all the new chunks in a list, sorted by LOD levels. LOD 0 = the highest resolution 4 chunks
		//Sent as the stardAdress argument, which is supposed to be sorted clockwise, starting from the left bottom

		List<Vector2>[] ComputeNewChunksAddresses(Vector2[] startAddress)
		{
			List<Vector2>[] ChunksLayered = new List<Vector2>[detailLevels.Length];

			for (int i = 0; i < detailLevels.Length; i++)
			{
				ChunksLayered[i] = new List<Vector2>();
			}
			string result = "";
			
			//Every item of chunkslayered represents a LOD level, starting with the highest LOD 0. 
			//We first put our 4 first chunks in the level 0 LOD array
			ChunksLayered[0].Add(startAddress[0]);
			ChunksLayered[0].Add(startAddress[1]);
			ChunksLayered[0].Add(startAddress[2]);
			ChunksLayered[0].Add(startAddress[3]);
			
			//We will now proceed to add all subsequent chunks
			for (int x = 1; x < detailLevels.Length;x++)
				{
					Vector2 leftBottom = ChunksLayered[x-1][0] - Vector2.One;
					Vector2 leftTop = ChunksLayered[x-1][1] + new Vector2(-1,1);
					Vector2 rightTop = ChunksLayered[x-1][2] + Vector2.One;
					Vector2 rightBottom = ChunksLayered[x-1][3] + new Vector2(1,-1);

					ChunksLayered[x].Add(leftBottom); 
					ChunksLayered[x].Add(leftTop); 
					ChunksLayered[x].Add(rightTop); 
					ChunksLayered[x].Add(rightBottom);
					float difference = ChunksLayered[x][3].X - ChunksLayered[x][1].X;
					//GD.Print("Difference is " + difference);
					for (int i = 0; i < difference-1;i++)
						{
							ChunksLayered[x].Add(new Vector2(leftBottom.X+1+i,leftBottom.Y));
							ChunksLayered[x].Add(new Vector2(leftBottom.X, leftBottom.Y+1+i));
							ChunksLayered[x].Add(new Vector2(rightTop.X-1-i, rightTop.Y));
							ChunksLayered[x].Add(new Vector2(rightTop.X, rightTop.Y-1-i));
						}
				}
			//GD.Print(result);
			return ChunksLayered;
		}
		public void OnBouncerockEvent(EvtCameraChanged evt)
		{
		   Viewer = evt.NewCamera;
		}

	}

	[System.Serializable]
	public struct LODInfo 
	{
		public int LodLevel;
		//public float visibleDstThreshold;

		public int AdjacencyLevel;

		public bool HasCollider;


		/*public float sqrVisibleDstThreshold 
		{
			get 
			{
				return visibleDstThreshold * visibleDstThreshold;
			}
		}*/
	}
}
