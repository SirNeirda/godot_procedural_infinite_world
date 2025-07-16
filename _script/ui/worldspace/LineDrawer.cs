using Godot;
using System;
using System.Collections.Generic;

namespace Bouncerock.UI
{
	public partial class LineDrawer : WorldSpaceUI
	{
		// Called when the node enters the scene tree for the first time.
		Line2D child;

		//protected List

		public override void _Ready()
		{}

		public static MeshInstance3D DrawLine3D(Vector3 start, Vector3 end, Color color)
		{
			// Create the MeshInstance3D
			MeshInstance3D meshInstance = new MeshInstance3D();

			// Create the ArrayMesh
			ArrayMesh arrayMesh = new ArrayMesh();

			// Create the material
			StandardMaterial3D material = new StandardMaterial3D
			{
				ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
				AlbedoColor = color
			};

			// Create arrays for the mesh
			var vertices = new Vector3[] { start, end };
			var indices = new int[] { 0, 1 };
			var colors = new Color[] { color, color };

			// Create the arrays
			var arrays = new Godot.Collections.Array();
			arrays.Resize((int)ArrayMesh.ArrayType.Max);
			arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
			arrays[(int)ArrayMesh.ArrayType.Index] = indices;
			arrays[(int)ArrayMesh.ArrayType.Color] = colors;

			// Add surface from arrays
			arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, arrays);

			// Assign the mesh to the MeshInstance3D
			meshInstance.Mesh = arrayMesh;
			meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;


			// Assign the material
			if (arrayMesh.GetSurfaceCount() > 0)
			{
				arrayMesh.SurfaceSetMaterial(0, material);
			}
			
			// Return the MeshInstance3D
			return meshInstance;

		}

		public static MeshInstance3D DrawSphereOpaque3D(Vector3 position, float radius, Color color)
		{
			 // Create the MeshInstance3D
			MeshInstance3D meshInstance = new MeshInstance3D();

			// Create the SphereMesh and set its properties
			SphereMesh sphereMesh = new SphereMesh
			{
				Radius = radius,
				Height = radius * 2
			};

			// Create the material
			StandardMaterial3D material = new StandardMaterial3D
			{
				ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
				AlbedoColor = color
			};

			// Assign the mesh and material to the MeshInstance3D
			meshInstance.Mesh = sphereMesh;
			meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
			meshInstance.GlobalTransform = new Transform3D(Basis.Identity, position);

			// Assign the material to the mesh
			sphereMesh.SurfaceSetMaterial(0, material);

			// Return the MeshInstance3D
			return meshInstance;

		}


		public static MeshInstance3D DrawAABB(Vector3 center, Vector3 size, Color color)
		{
			 // Create the MeshInstance3D
			MeshInstance3D meshInstance = new MeshInstance3D();

			// Create the BoxMesh and set its properties
			BoxMesh boxMesh = new BoxMesh
			{
				Size = size
			};

			// Create the material
			StandardMaterial3D material = new StandardMaterial3D
			{
				ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
				Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
				AlbedoColor = color
			};

			// Assign the mesh and material to the MeshInstance3D
			meshInstance.Mesh = boxMesh;
			meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
			meshInstance.GlobalTransform = new Transform3D(Basis.Identity, center);

			// Assign the material to the mesh
			boxMesh.SurfaceSetMaterial(0, material);

			// Return the MeshInstance3D
			return meshInstance;

		}
	}
}