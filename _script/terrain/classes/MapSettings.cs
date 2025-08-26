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
			pass1.VerticalScale = 100;
			pass1.HorizontalScale = Vector2.One*5000;
			pass1.Frequency = 2;
			pass1.Octaves = 2;

			TerrainPass pass2 = new TerrainPass();
			pass2.VerticalScale = 20;
			pass2.HorizontalScale = Vector2.One*210;
			//pass2.Octaves = 3;

			TerrainPass pass3 = new TerrainPass();
			pass3.VerticalScale = 10;
			pass3.HorizontalScale = Vector2.One*100f;
			pass1.Octaves = 2;
			pass1.Frequency = 5;
			pass3.Contrast = 1.5f;

			Passes.Add(pass1);
			Passes.Add(pass2);
			Passes.Add(pass3);

			NaturalObjects = new List<NaturalObject>();
			NaturalObject newObj = new NaturalObject();
			newObj.ObjectName = "tree_2";
			newObj.Concentration = 3;
			newObj.RandomizeTiltAngle = 5;
			newObj.RandomizeYRotation = true;
			newObj.MinSize = 0.8f;
			newObj.MaxSize = 1.2f;

			NaturalObjects = new List<NaturalObject>();
			NaturalObject tree = new NaturalObject();
			tree.ObjectName = "tree_3";
			tree.Concentration = 2f;
			tree.RandomizeTiltAngle = 5;
			tree.RandomizeYRotation = true;
			tree.MinSize = 0.8f;
			tree.MaxSize = 1.2f;


			NaturalObject newObjbush = new NaturalObject();
			newObjbush.ObjectName = "bush_berries";
			newObjbush.Concentration = 3;
			newObjbush.MinSize = 0.8f;
			newObjbush.MaxSize = 1.3f;

			NaturalObject newObj2 = new NaturalObject();
			newObj2.ObjectName = "wall";
			newObj2.Concentration = 1;

			NaturalObject newObj3 = new NaturalObject();
			newObj3.ObjectName = "stoneandplant";
			newObj3.Concentration = 5f;

			NaturalObject pine = new NaturalObject();
			pine.ObjectName = "pine_tree_1";
			pine.Concentration = 0.01f;


			NaturalObject newObstone = new NaturalObject();
			newObstone.ObjectName = "schroom";
			newObstone.Concentration = 0.1f;
			newObstone.MinSize = 0.8f;
			newObstone.MaxSize = 2f;

			NaturalObjects.Add(newObj);
			NaturalObjects.Add(newObj2);
			NaturalObjects.Add(newObjbush);
			NaturalObjects.Add(newObj3);
			NaturalObjects.Add(newObstone);
			NaturalObjects.Add(tree);
			NaturalObjects.Add(pine);

			Pathway = new PathwaySettings();
			GameplayObject powerUp1 = new GameplayObject();
			powerUp1.ObjectName = "power_up";
			powerUp1.Concentration = 3;
			powerUp1.Levitation = 6;
			GameplayObjects.Add(powerUp1);
		}

	}




	public class TerrainPass
	{
		public Vector2 HorizontalScale = Vector2.One*2; // the heightmap xy scale
		public float VerticalScale = 50; // acts as multiplier to the normalized noise map


		public float MaxHeight = 50; // absolute max is 200
		public float MinHeight = -20; // absolute max is 200

		public int Octaves = 1;

		public int Frequency = 1;

		public float Contrast = 1;
	}
	public class PathwaySettings
	{
		public float PathwayScale = 0.1f;      // Controls the noise scale for pathways
		public float Threshold = 0.1f;         // Defines the threshold for pathway existence in the noise
		public float FlattenHeight = 0.5f;     // Defines the flattened height for pathways
		public bool SmoothEdges = true;        // Whether to smooth edges around the pathway
		public int TransitionRadius = 10;       // The radius around pathways where smoothing occurs
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
		public float Concentration = 1f;
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

		public WorldItemData BaseData = new WorldItemData();



	}

	public class GameplayObject : SpawnedObject
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