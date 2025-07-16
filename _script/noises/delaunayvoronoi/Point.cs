using System.Collections.Generic;
using Godot;

namespace Bouncerock.Procedural
{
    public class DelaunayPoint
    {
        /// <summary>
        /// Used only for generating a unique ID for each instance of this class that gets generated
        /// </summary>
        private static int _counter;

        /// <summary>
        /// Used for identifying an instance of a class; can be useful in troubleshooting when geometry goes weird
        /// (e.g. when trying to identify when Triangle objects are being created with the same Point object twice)
        /// </summary>
        private readonly int _instanceId = _counter++;

       // public double X { get; }
        //public double Y { get; }
        public Vector2 position = new Vector2();
        public HashSet<DelaunayTriangle> AdjacentTriangles { get; } = new HashSet<DelaunayTriangle>();

        public DelaunayPoint(float x, float y)
        {
            position.X = x;
            position.Y = y;
        }

        public override string ToString()
        {
            // Simple way of seeing what's going on in the debugger when investigating weirdness
            return $"{nameof(DelaunayPoint)} {_instanceId} {position.X:0.##}@{position.Y:0.##}";
        }

        
    }
}