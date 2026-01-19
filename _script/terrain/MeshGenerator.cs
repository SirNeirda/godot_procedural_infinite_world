////////////////////////////////////////////////////////////////////////////////////////////
/// This script is part of the project "Infinite Runner",
/// a 3D procedural world generation project for Godot
///
/// By Adrien Pierret
/// 
/// MeshGenerator: This script creates mesh from the data given. Most of it comes directly from "Procedural Landmass"
/// by Sebastian Lague, but adapted to Godot. One important addition is the generation of vertex colors, which we'll use 
/// for texture blending.
/// ///////////////////////////////////////////////////////////////////////////////////////

using Godot;
using Godot.Collections;
using Bouncerock;
using System.Threading.Tasks;

namespace Bouncerock.Terrain
{
	public static class MeshGenerator
	{
		public static async Task<MeshData> GenerateTerrainMeshAsync(Map map, int levelOfDetail)
		{
			if (map.heightMap == null) { GD.Print("Null heightmap"); return null; }
			int skipIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
			int numVertsPerLine = TerrainMeshSettings.numVertsPerLine;
			Vector2 topLeft = new Vector2(-1, 1) * TerrainMeshSettings.meshWorldSize / 2f;
			MeshData meshData = new MeshData(TerrainMeshSettings.numVertsPerLine, skipIncrement, TerrainMeshSettings.useFlatShading);

			int[,] vertexIndicesMap = new int[numVertsPerLine, numVertsPerLine];
			int meshVertexIndex = 0;
			int outOfMeshVertexIndex = -1;

			await Task.Run(() =>
			{
				for (int y = 0; y < numVertsPerLine; y++)
				{
					for (int x = 0; x < numVertsPerLine; x++)
					{
						bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
						bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);
						if (isOutOfMeshVertex)
						{
							vertexIndicesMap[x, y] = outOfMeshVertexIndex;
							outOfMeshVertexIndex--;
						}
						else if (!isSkippedVertex)
						{
							vertexIndicesMap[x, y] = meshVertexIndex;
							meshVertexIndex++;
						}
					}
				}
			});

			await Task.Run(() =>
			{
				for (int y = 0; y < numVertsPerLine; y++)
				{
					for (int x = 0; x < numVertsPerLine; x++)
					{
						bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);

						if (!isSkippedVertex)
						{
							bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
							bool isMeshEdgeVertex = (y == 1 || y == numVertsPerLine - 2 || x == 1 || x == numVertsPerLine - 2) && !isOutOfMeshVertex;
							bool isMainVertex = (x - 2) % skipIncrement == 0 && (y - 2) % skipIncrement == 0 && !isOutOfMeshVertex && !isMeshEdgeVertex;
							bool isEdgeConnectionVertex = (y == 2 || y == numVertsPerLine - 3 || x == 2 || x == numVertsPerLine - 3) && !isOutOfMeshVertex && !isMeshEdgeVertex && !isMainVertex;

							int vertexIndex = vertexIndicesMap[x, y];
							Vector2 percent = new Vector2(x - 1, y - 1) / (numVertsPerLine - 3);
							Vector2 vertexPosition2D = topLeft + new Vector2(percent.X, -percent.Y) * TerrainMeshSettings.meshWorldSize;

							float height = map.heightMap[x, y];

							if (isEdgeConnectionVertex)
							{
								bool isVertical = x == 2 || x == numVertsPerLine - 3;
								int dstToMainVertexA = ((isVertical) ? y - 2 : x - 2) % skipIncrement;
								int dstToMainVertexB = skipIncrement - dstToMainVertexA;
								float dstPercentFromAToB = dstToMainVertexA / (float)skipIncrement;

								float heightMainVertexA = map.heightMap[(isVertical) ? x : x - dstToMainVertexA, (isVertical) ? y - dstToMainVertexA : y];
								float heightMainVertexB = map.heightMap[(isVertical) ? x : x + dstToMainVertexB, (isVertical) ? y + dstToMainVertexB : y];

								height = heightMainVertexA * (1 - dstPercentFromAToB) + heightMainVertexB * dstPercentFromAToB;
							}

							meshData.AddVertex(new Vector3(vertexPosition2D.X, height, vertexPosition2D.Y), percent, vertexIndex);

							bool createTriangle = x < numVertsPerLine - 1 && y < numVertsPerLine - 1 && (!isEdgeConnectionVertex || (x != 2 && y != 2));

							if (createTriangle)
							{
								int currentIncrement = (isMainVertex && x != numVertsPerLine - 3 && y != numVertsPerLine - 3) ? skipIncrement : 1;

								int a = vertexIndicesMap[x, y];
								int b = vertexIndicesMap[x + currentIncrement, y];
								int c = vertexIndicesMap[x, y + currentIncrement];
								int d = vertexIndicesMap[x + currentIncrement, y + currentIncrement];
								meshData.AddTriangle(a, d, b);
								meshData.AddTriangle(a, c, d);
							}
						}
					}
				}
			});

			meshData.ProcessMesh();

			return meshData;
		}


	}

	public class MeshData
	{
		Vector3[] vertices;
		int[] triangles;
		Vector2[] uvs;
		Vector3[] bakedNormals;

		// Vertex colors for slope-based coloring
		Color[] vertexColors; // Added: Array for storing vertex colors

		Vector3[] outOfMeshVertices;
		int[] outOfMeshTriangles;

		int triangleIndex;
		int outOfMeshTriangleIndex;

		bool useFlatShading;

		public MeshData(int numVertsPerLine, int skipIncrement, bool useFlatShading)
		{
			this.useFlatShading = useFlatShading;

			int numMeshEdgeVertices = (numVertsPerLine - 2) * 4 - 4;
			int numEdgeConnectionVertices = (skipIncrement - 1) * (numVertsPerLine - 5) / skipIncrement * 4;
			int numMainVerticesPerLine = (numVertsPerLine - 5) / skipIncrement + 1;
			int numMainVertices = numMainVerticesPerLine * numMainVerticesPerLine;

			vertices = new Vector3[numMeshEdgeVertices + numEdgeConnectionVertices + numMainVertices];
			uvs = new Vector2[vertices.Length];
			vertexColors = new Color[vertices.Length]; // Added: Initialize vertex color array

			int numMeshEdgeTriangles = 8 * (numVertsPerLine - 4);
			int numMainTriangles = (numMainVerticesPerLine - 1) * (numMainVerticesPerLine - 1) * 2;
			triangles = new int[(numMeshEdgeTriangles + numMainTriangles) * 3];

			outOfMeshVertices = new Vector3[numVertsPerLine * 4 - 4];
			outOfMeshTriangles = new int[24 * (numVertsPerLine - 2)];
		}

		public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
		{
			if (vertexIndex < 0)
			{
				outOfMeshVertices[-vertexIndex - 1] = vertexPosition;
			}
			else
			{
				vertices[vertexIndex] = vertexPosition;
				uvs[vertexIndex] = uv;


			}
		}

		private Color ColorFromSlope(float slope, float height)
		{
			float angleDegrees = Mathf.RadToDeg(Mathf.Acos(1.0f - slope));
			//GD.Print("Slope: " + angleDegrees + "° // Height: " + height);

			// Red = flatter is brighter
			float red = Mathf.Clamp(1.0f - slope * 250.0f , 0.0f, 1.0f);

			// Green = height mapped from -200 to +200
			float green = Mathf.Clamp((height + 200.0f) / 400.0f, 0.0f, 1.0f);

			// Blue = fade in from height 2.0 to 0.0
			float blue = Mathf.Clamp((5.0f - height) / 2.0f, 0.0f, 1.0f);
			if (height <0) { green = 1; }
			if (blue >= 0.9f)
			{
				red = 0;
				green = 0;
			}
			if (red >= 0.9f)
			{
				green = 0;
				blue = 0;
			}
			if (green >= 0.9f)
			{
				red = 0;
				blue = 0;
			}
			return new Color(red, green, blue);
		}

		public void AddTriangle(int a, int b, int c)
		{
			if (a < 0 || b < 0 || c < 0)
			{
				outOfMeshTriangles[outOfMeshTriangleIndex] = a;
				outOfMeshTriangles[outOfMeshTriangleIndex + 1] = b;
				outOfMeshTriangles[outOfMeshTriangleIndex + 2] = c;
				outOfMeshTriangleIndex += 3;
			}
			else
			{
				triangles[triangleIndex] = a;
				triangles[triangleIndex + 1] = b;
				triangles[triangleIndex + 2] = c;
				triangleIndex += 3;
			}
		}

		/*public void AddVertexColor(Color color, int vertexIndex)
		{
			if (vertexIndex >= 0)
			{
				vertexColors[vertexIndex] = color;
			}
		}*/

		(Vector3[], Color[]) CalculateNormals()
		{

			Vector3[] vertexNormals = new Vector3[vertices.Length];
			Color[] vertexColors = new Color[vertices.Length];
			int triangleCount = triangles.Length / 3;
			for (int i = 0; i < triangleCount; i++)
			{
				int normalTriangleIndex = i * 3;
				int vertexIndexA = triangles[normalTriangleIndex];
				int vertexIndexB = triangles[normalTriangleIndex + 1];
				int vertexIndexC = triangles[normalTriangleIndex + 2];

				Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
				vertexNormals[vertexIndexA] += triangleNormal;
				vertexNormals[vertexIndexB] += triangleNormal;
				vertexNormals[vertexIndexC] += triangleNormal;


			}

			int borderTriangleCount = outOfMeshTriangles.Length / 3;
			for (int i = 0; i < borderTriangleCount; i++)
			{
				int normalTriangleIndex = i * 3;
				int vertexIndexA = outOfMeshTriangles[normalTriangleIndex];
				int vertexIndexB = outOfMeshTriangles[normalTriangleIndex + 1];
				int vertexIndexC = outOfMeshTriangles[normalTriangleIndex + 2];

				Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
				if (vertexIndexA >= 0)
				{
					vertexNormals[vertexIndexA] += triangleNormal;
				}
				if (vertexIndexB >= 0)
				{
					vertexNormals[vertexIndexB] += triangleNormal;
				}
				if (vertexIndexC >= 0)
				{
					vertexNormals[vertexIndexC] += triangleNormal;
				}
			}


			for (int i = 0; i < vertexNormals.Length; i++)
			{
				vertexNormals[i].Normalized();
			}
			for (int i = 0; i < vertexNormals.Length; i++)
			{
				vertexNormals[i] = vertexNormals[i].Normalized();
				float slope = 1.0f - Mathf.Abs(vertexNormals[i].Dot(Vector3.Up));
				float y = vertices[i].Y;
				vertexColors[i] = ColorFromSlope(slope, y);
				//GD.Print(vertexColors[i]);
			}
			return (vertexNormals, vertexColors);

		}

		Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
		{
			Vector3 pointA = (indexA < 0) ? outOfMeshVertices[-indexA - 1] : vertices[indexA];
			Vector3 pointB = (indexB < 0) ? outOfMeshVertices[-indexB - 1] : vertices[indexB];
			Vector3 pointC = (indexC < 0) ? outOfMeshVertices[-indexC - 1] : vertices[indexC];

			Vector3 sideAB = pointB - pointA;
			Vector3 sideAC = pointC - pointA;

			return sideAC.Cross(sideAB).Normalized();
		}

		public void ProcessMesh()
		{
			BakeNormals();
		}

		void BakeNormals()
		{
			(bakedNormals, vertexColors) = CalculateNormals();
		}


		public async Task<ArrayMesh> CreateMesh()
		{
			ArrayMesh arrayMesh = new ArrayMesh();
			await Task.Run(() =>
			{
				//GD.Print("Number of verticles : " + vertices.Length);
				Array array = new();
				array.Resize((int)ArrayMesh.ArrayType.Max);
				array[(int)ArrayMesh.ArrayType.Vertex] = Variant.CreateFrom(vertices);
				array[(int)ArrayMesh.ArrayType.TexUV] = Variant.CreateFrom(uvs);
				array[(int)ArrayMesh.ArrayType.Index] = Variant.CreateFrom(triangles);
				array[(int)ArrayMesh.ArrayType.Normal] = Variant.CreateFrom(bakedNormals);
				array[(int)ArrayMesh.ArrayType.Color] = Variant.CreateFrom(vertexColors);
				arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array);
				arrayMesh.RegenNormalMaps();
				//arrayMesh.CreateTrimeshShape();
				//GD.Print("Creating mesh DONE"); 
			});
			return arrayMesh;
		}

	}
}