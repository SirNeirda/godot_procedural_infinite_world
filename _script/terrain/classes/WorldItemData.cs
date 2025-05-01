using Godot;
using System.Collections;
using System.Collections.Generic;

namespace Bouncerock.Terrain
{

    [System.Serializable]
    public struct WorldItemData
        {
            public string name;
            public Vector2 GridLocation;
            public string Hash;
            public Vector3 Size;

            public float Elevation;

            public void SetElevation(float elevation)
            {
                Elevation = elevation;
            }
        }
}