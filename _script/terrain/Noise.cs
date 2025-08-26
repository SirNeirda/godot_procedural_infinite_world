using Godot;
using System.Collections.Generic;
using Bouncerock.Procedural;
using Bouncerock.Terrain;

namespace Bouncerock
{
	public static class Noise
	{

		public enum NormalizeModes { Local, Global };
		public enum NoiseTypes { SimplexSmooth, Voronoi };

		public enum ReturnTypes { Raster, Vector };



		//This is the noise genera

		public static float[,] GenerateNoiseMapSimplex(
	int mapWidth,
	int mapHeight,
	NoiseSettingsSimplexSmooth pass,
	Vector2 sampleCentre)
		{
			float[,] noiseMap = new float[mapWidth, mapHeight];

			float halfWidth = mapWidth / 2f;
			float halfHeight = mapHeight / 2f;

			FastNoiseLite noise = new FastNoiseLite();
			noise.Seed = TerrainManager.Instance.Seed;
			noise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
			noise.FractalOctaves = pass.octaves;
			noise.Frequency = pass.frequency;

			for (int y = 0; y < mapHeight; y++)
			{
				for (int x = 0; x < mapWidth; x++)
				{
					// apply horizontal XY scale
					float sampleX = ((x - halfWidth + sampleCentre.X) / pass.offset.X);
					float sampleY = ((y - halfHeight + sampleCentre.Y) / pass.offset.Y);

					// raw noise in [-1,1]
					float noiseValue = noise.GetNoise2D(sampleX, sampleY);


					// apply vertical scale
					float heightValue = noiseValue * pass.scale.X;

					// clamp final value

					noiseMap[x, y] = heightValue;
				}
			}

			return noiseMap;
		}


		public static float[,] GenerateNoiseMapSimplex_deprecated(int mapWidth, int mapHeight, NoiseSettingsSimplexSmooth settings, Vector2 sampleCentre)
		{
			float[,] noiseMap = new float[mapWidth, mapHeight];


			float halfWidth = mapWidth / 2f;
			float halfHeight = mapHeight / 2f;
			//float noiseHeight = 0;
			FastNoiseLite noise = new FastNoiseLite();
			noise.Seed = TerrainManager.Instance.Seed;
			noise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
			noise.FractalOctaves = settings.octaves;
			noise.Frequency = settings.frequency;
			for (int y = 0; y < mapHeight; y++)
			{
				for (int x = 0; x < mapWidth; x++)
				{
					//float sampleX = (x - halfWidth + settings.offset.X) / settings.scale;//x+(settings.offset.x/;
					//float sampleY = (y - halfWidth + settings.offset.Y) / settings.scale;//y+settings.offset.y;

					//float perlinValue = noise.GetNoise2D(sampleX, sampleY);
					//noiseMap[x, y] = perlinValue;//Mathf.InverseLerp(-1,1, perlinValue);
				}
			}

			return noiseMap;
		}

		public static float[,] GenerateNoiseMapVoronoi(int mapWidth, int mapHeight, NoiseSettingsVoronoi settings, Vector2 sampleCentre)
		{
			float[,] noiseMap = new float[mapWidth, mapHeight];
			DelaunayTriangulator delaunay = new DelaunayTriangulator();
			IEnumerable<DelaunayPoint> delaunayPoints = delaunay.GeneratePoints(settings.concentration, settings.offset, mapWidth - 1, mapWidth - 1);
			return noiseMap;
		}

	}

	[System.Serializable]
	public class NoiseSettings
	{
		public int seed = 1234;
		public Vector2 offset = Vector2.Zero;
		public Vector2 scale = Vector2.Zero;
	}

	public class NoiseSettingsSimplexSmooth : NoiseSettings
	{


		public int octaves = 1;
		public float frequency = .6f;

	}

	public class NoiseSettingsVoronoi : NoiseSettings
	{
		public int concentration = 1;
		public Noise.ReturnTypes ReturnType = Noise.ReturnTypes.Vector;
	}

}