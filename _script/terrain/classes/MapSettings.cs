using Godot;
using System.Collections;
using System.Collections.Generic;

namespace Bouncerock.Terrain
{
	
	public class MapGenerationSettings 
	{

		//public int ChunkMeshSize = 240;

		//public float ChunkWorldSize = 120;

		//This will be the highest and lowest possible values in the terrain.

		public int HighestPoint = 200;
		public int LowestPoint = -200;

		public List<TerrainPass> Passes;

		public List<NaturalObject> NaturalObjects = new List<NaturalObject>();

		public List<GameplayObject> GameplayObjects = new List<GameplayObject>();

		public PathwaySettings Pathway;

		//public List<Biome> Biomes = new List<Biome>(); //WIP


		//Tweak these values if you want a different terrain to be generated
		//You can add new passes to make the terrain more subtle, each pass will be superposed to the latest

		public void DefaultValues()
			{
				Passes = new List<TerrainPass>();
				TerrainPass pass1 = new TerrainPass();
				pass1.VerticalScale = 10000;
				pass1.HorizontalScale = 200;

				TerrainPass pass2 = new TerrainPass();
				pass2.VerticalScale = 1000;
				pass2.HorizontalScale = 70;
				pass2.Octaves = 3;

				TerrainPass pass3 = new TerrainPass();
				pass2.VerticalScale = 100;
				pass2.HorizontalScale = 20;
				pass2.Contrast = 3;

				Passes.Add(pass1);
				Passes.Add(pass2);
				Passes.Add(pass3);

				NaturalObjects = new List<NaturalObject>();
				NaturalObject newObj = new NaturalObject();
				newObj.ObjectName = "tree_2";
				newObj.Concentration = 7;
				NaturalObject newObj2 = new NaturalObject();
				newObj2.ObjectName = "wall";
				newObj2.Concentration = 2;
				NaturalObjects.Add(newObj);
				NaturalObjects.Add(newObj2);
				Pathway = new PathwaySettings();
				GameplayObject powerUp1 = new GameplayObject();
				powerUp1.ObjectName = "power_up";
				powerUp1.Concentration = 10;
				powerUp1.Levitation =6;
				GameplayObjects.Add(powerUp1);
			}
		
	}

	

	public class TerrainPass
	{
		public float HorizontalScale = 2;
		public float VerticalScale = 50;

		public int Octaves = 1;

		public float Contrast = 1;
	}
	public class PathwaySettings
		{
			public float PathwayScale = 0.1f;      // Controls the noise scale for pathways
			public float Threshold = 0.1f;         // Defines the threshold for pathway existence in the noise
			public float FlattenHeight = 0.5f;     // Defines the flattened height for pathways
			public bool SmoothEdges = true;        // Whether to smooth edges around the pathway
			public int TransitionRadius =10;       // The radius around pathways where smoothing occurs
		}

	public class Biome
	{
		public string Name;
		public List<NaturalObject> NaturalObjects = new List<NaturalObject>();
	}

	public struct HeightMapSettings
	{
		public NoiseSettings noiseSettings;
		public float heightMultiplier;
	}

	public class SpawnedObject
	{
		public string ObjectName = "ObjectName";

		 public string Path;

		 //Concentration should be the number of this item per chunk, if inferior to 1, should be the percentage of chance to encounter it per chunk. 
		 //Therefore, if the number is inferior to 1, we should use a simple random number to determine presence, and if superior to 1, use poisson algorithm
		public float Concentration=1f;
	}

	public class NaturalObject : SpawnedObject
	{
		public WorldItem relevantItem;
		/*public string ObjectName = "ObjectName";

		 public string Path;

		 //Concentration should be the number of this item per chunk, if inferior to 1, should be the percentage of chance to encounter it per chunk. 
		 //Therefore, if the number is inferior to 1, we should use a simple random number to determine presence, and if superior to 1, use poisson algorithm
		public float Concentration=1f;*/

		public float MinSize = 0.9f;
		public float MaxSize = 1.1f;
        public bool RandomizeYRotation = true;

        public float RandomizeTiltAngle = 10;
        public float MinimumSpawnAltitude = 0;

        public float MaximumSpawnAltitude = 100;

        public WorldItemData BaseData= new WorldItemData();

        

	}

	public class GameplayObject: SpawnedObject
	{
		public WorldItem relevantItem;
		//public string ObjectName = "ObjectName";

		public float Levitation = 2;

		 //public string Path;

		 //Concentration should be the number of this item per chunk, if inferior to 1, should be the percentage of chance to encounter it per chunk. 
		 //Therefore, if the number is inferior to 1, we should use a simple random number to determine presence, and if superior to 1, use poisson algorithm
		//public float Concentration=1f;

		public Node3D ObjectToSpawn;
	}

	
}