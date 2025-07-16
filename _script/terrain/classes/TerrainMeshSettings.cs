using Godot;
using System.Collections;

namespace Bouncerock.Terrain
{

	public class TerrainMeshSettings
	{
		//public Color TestVariable;
		public int numSupportedLODs = 5;
		public int numSupportedChunkSizes = 9;
		//public int numSupportedFlatshadedChunkSizes = 3;
		public static readonly int[] supportedChunkSizes = {48,72,96,120,144,168,192,216,240};
		
		//public static int ChunkSize = 120;

		public static float meshScale = 1f;
		public static bool useFlatShading = false;

		public static int chunkSizeIndex = 0;
		//public static int flatshadedChunkSizeIndex;


		// num verts per line of mesh rendered at LOD = 0. Includes the 2 extra verts that are excluded from final mesh, but used for calculating normals
		public static int numVertsPerLine 
		{
			get 
			{
				return supportedChunkSizes [chunkSizeIndex] + 5;
			}
		}

		public static float meshWorldSize {
			get
			{
				return (numVertsPerLine - 3) * meshScale;
			}
		}

		public static float chunkMeshSize {
			get
			{
				return supportedChunkSizes[chunkSizeIndex];
			}
		}
	}
}