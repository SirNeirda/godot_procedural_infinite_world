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

		public List<WorldItemSettings> NaturalObjects = new List<WorldItemSettings>();

		public List<WorldItemSettings> GameplayObjects = new List<WorldItemSettings>();

		public PathwaySettings Pathway;

		//public List<Biome> Biomes = new List<Biome>(); //WIP


		//Tweak these values if you want a different terrain to be generated
		//You can add new passes to make the terrain more subtle, each pass will be superposed to the latest


		



		public void DefaultValues()
		{
			Passes = new List<TerrainPass>();
			TerrainPass pass1 = new TerrainPass();
			pass1.VerticalScale = 120;
			pass1.HorizontalScale = Vector2.One*3000;
			pass1.Frequency = 2;
			pass1.Octaves = 2;
			pass1.BlendType = TerrainPass.BlendTypes.Mix;

			TerrainPass pass2 = new TerrainPass();
			pass2.VerticalScale = 50;
			pass2.HorizontalScale = Vector2.One*700;
			pass2.BlendType = TerrainPass.BlendTypes.Mix;
			pass2.Octaves = 3;
			pass1.Frequency = 3;

			TerrainPass pass3 = new TerrainPass();
			pass3.VerticalScale = 15;
			pass3.HorizontalScale = Vector2.One*300;
			pass3.Octaves = 2;
			pass3.Frequency = 5;
			pass3.Contrast = 1f;
			pass3.MinHeight = -20f;
			pass3.MaxHeight = 10f;
			pass3.BlendValue = 0.3f;
			pass3.BlendType = TerrainPass.BlendTypes.Add;

			TerrainPass pass4 = new TerrainPass();
			pass4.VerticalScale = 70;
			pass4.HorizontalScale = Vector2.One*150;
			pass4.Octaves = 2;
			pass4.Frequency = 5;
			pass4.Contrast = 1f;
			pass4.MinHeight = 20;
			pass4.MaxHeight = 200;
			pass4.BlendType = TerrainPass.BlendTypes.Add;

			Passes.Add(pass1);
			Passes.Add(pass2);
			Passes.Add(pass3);
			Passes.Add(pass4);

			NaturalObjects = new List<WorldItemSettings>();
			WorldItemSettings newObj = new WorldItemSettings();
			newObj.Name = "tree_2";
			newObj.Path = "tree_2";
			newObj.Concentration = 3f;
			newObj.RandomizeTiltAngle = 5;
			newObj.RandomizeYRotation = true;
			newObj.MinSize = 0.8f;
			newObj.MinimumSpawnAltitude = 15f;
			newObj.MaxSize = 1.2f;
			newObj.ItemType = WorldItemSettings.ItemTypes.Static;

			NaturalObjects = new List<WorldItemSettings>();
			WorldItemSettings tree = new WorldItemSettings();
			tree.Name = "tree_3";
			tree.Path = "tree_3";
			tree.Concentration = 1.7f;
			tree.RandomizeTiltAngle = 5;
			tree.RandomizeYRotation = true;
			tree.MinimumSpawnAltitude = 8f;
			tree.MinSize = 0.8f;
			tree.MaxSize = 1.7f;
			tree.ItemType = WorldItemSettings.ItemTypes.Static;


			WorldItemSettings newObjbush = new WorldItemSettings();
			newObjbush.Name = "bush_berries";
			newObjbush.Path = "bush_berries";
			newObjbush.Concentration = 3;
			newObjbush.MinSize = 0.8f;
			newObjbush.MaxSize = 1.3f;
			newObjbush.ItemType = WorldItemSettings.ItemTypes.Static;

			WorldItemSettings newObj2 = new WorldItemSettings();
			newObj2.Name = "wall";
			newObj2.Path = "wall";
			newObj2.Concentration = 1;
			newObj2.ItemType = WorldItemSettings.ItemTypes.Static;

			WorldItemSettings newObj3 = new WorldItemSettings();
			newObj3.Name = "stoneandplant";
			newObj3.Path = "stoneandplant";
			newObj3.Concentration = 5f;
			newObj3.ItemType = WorldItemSettings.ItemTypes.Static;

			WorldItemSettings pine = new WorldItemSettings();
			pine.Name = "pine_tree_1";
			pine.Path = "pine_tree_1";
			pine.Concentration = 0.01f;
			pine.MinimumSpawnAltitude = 30f;
			pine.ItemType = WorldItemSettings.ItemTypes.Static;


			WorldItemSettings newObstone = new WorldItemSettings();
			newObstone.Name = "schroom";
			newObstone.Path = "schroom";
			newObstone.Concentration = 0.1f;
			newObstone.MinSize = 0.8f;
			newObstone.MaxSize = 2f;
			newObstone.ItemType = WorldItemSettings.ItemTypes.Static;

			WorldItemSettings palm = new WorldItemSettings();
			palm.Name = "palm_tree_1";
			palm.Path = "palm_tree_1";
			palm.Concentration = 2.2f;
			palm.RandomizeTiltAngle = 3;
			palm.MinimumSpawnAltitude = -0.2f;
			palm.MaximumSpawnAltitude = 6f;
			palm.RandomizeYRotation = true;
			palm.MinSize = 1.5f;
			palm.MaxSize = 1.8f;
			palm.ItemType = WorldItemSettings.ItemTypes.Static;

			WorldItemSettings tower = new WorldItemSettings();
			tower.Name = "tower";
			tower.Path = "tower";
			tower.Concentration = 0.01f;
			tower.MinSize = 0.7f;
			tower.MaxSize = 0.7f;
			tower.MinimumSpawnAltitude = 10f;
			tower.RandomizeYRotation = false;
			tower.RandomizeTiltAngle = 0;
			tower.ItemType = WorldItemSettings.ItemTypes.Static;
			tower.Levitation = 1;

			NaturalObjects.Add(newObj);
			NaturalObjects.Add(newObj2);
			NaturalObjects.Add(newObjbush);
			NaturalObjects.Add(newObj3);
			NaturalObjects.Add(newObstone);
			NaturalObjects.Add(tree);
			NaturalObjects.Add(pine);
			NaturalObjects.Add(tower);
			NaturalObjects.Add(palm);

			Pathway = new PathwaySettings();
			WorldItemSettings powerUp1 = new WorldItemSettings();
			powerUp1.Name = "power_up";
			powerUp1.Path = "power_up";
			powerUp1.MinSize = 2;
			powerUp1.MaxSize = 2;
			powerUp1.Concentration = 3;
			powerUp1.Levitation = 6;
			GameplayObjects.Add(powerUp1);
		}

	}




	public class TerrainPass
	{
		public Vector2 HorizontalScale = Vector2.One*2; // the heightmap xy scale
		public float VerticalScale = 50; // acts as multiplier to the normalized noise map

		public float MaxHeight = 200; // absolute max is 200
		public float MinHeight = -200; // absolute max is 200

		public int Octaves = 1;

		public int Frequency = 1;

		public float Contrast = 1;

		public enum BlendTypes {None, Mix, Add, Substract}

		public BlendTypes BlendType = BlendTypes.None;

		public float BlendValue = 0.5f;

		public float[,] Mask = null;
	}
	public class PathwaySettings
	{
		public float Width = 2;
		public float MinElevation = 30;

		public float MaxElevation = 30;
	}


	public struct HeightMapSettings
	{
		public NoiseSettings noiseSettings;
		public float heightMultiplier;
	}

	/*public class SpawnedObject
	{
		public string ObjectName = "ObjectName";

		public string Path;

		//Concentration should be the number of this item per chunk, if inferior to 1, should be the percentage of chance to encounter it per chunk. 
		//Therefore, if the number is inferior to 1, we should use a simple random number to determine presence, and if superior to 1, use poisson algorithm
		public float Concentration = 1f;
	}

	public class NaturalObject : SpawnedObject
	{
		//public WorldItem relevantItem;

		public float MinSize = 0.9f;
		public float MaxSize = 1.1f;
		public bool RandomizeYRotation = true;

		public float RandomizeTiltAngle = 10;
		public float MinimumSpawnAltitude = 0;

		public float MaximumSpawnAltitude = 100;




	}

	public class GameplayObject : SpawnedObject
	{
		//public WorldItem relevantItem;
		//public string ObjectName = "ObjectName";

		public float Levitation = 2;

		//public string Path;

		//Concentration should be the number of this item per chunk, if inferior to 1, should be the percentage of chance to encounter it per chunk. 
		//Therefore, if the number is inferior to 1, we should use a simple random number to determine presence, and if superior to 1, use poisson algorithm
		//public float Concentration=1f;

		//public Node3D ObjectToSpawn;
	}*/


}