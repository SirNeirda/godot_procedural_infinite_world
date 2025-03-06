using System;
using System.Collections.Generic;
using System.Linq;

namespace Bouncerock.Procedural
{
    public class DelaunayTriangle
    {
        public DelaunayPoint[] Vertices { get; } = new DelaunayPoint[3];
        public DelaunayPoint Circumcenter { get; private set; }
        public double RadiusSquared;

        public IEnumerable<DelaunayTriangle> TrianglesWithSharedEdge {
            get {
                var neighbors = new HashSet<DelaunayTriangle>();
                foreach (var vertex in Vertices)
                {
                    var trianglesWithSharedEdge = vertex.AdjacentTriangles.Where(o =>
                    {
                        return o != this && SharesEdgeWith(o);
                    });
                    neighbors.UnionWith(trianglesWithSharedEdge);
                }

                return neighbors;
            }
        }

        public DelaunayTriangle(DelaunayPoint point1, DelaunayPoint point2, DelaunayPoint point3)
        {
            // In theory this shouldn't happen, but it was at one point so this at least makes sure we're getting a
            // relatively easily-recognised error message, and provides a handy breakpoint for debugging.
            if (point1 == point2 || point1 == point3 || point2 == point3)
            {
                throw new ArgumentException("Must be 3 distinct points");
            }

            if (!IsCounterClockwise(point1, point2, point3))
            {
                Vertices[0] = point1;
                Vertices[1] = point3;
                Vertices[2] = point2;
            }
            else
            {
                Vertices[0] = point1;
                Vertices[1] = point2;
                Vertices[2] = point3;
            }

            Vertices[0].AdjacentTriangles.Add(this);
            Vertices[1].AdjacentTriangles.Add(this);
            Vertices[2].AdjacentTriangles.Add(this);
            UpdateCircumcircle();
        }

        private void UpdateCircumcircle()
        {
            // https://codefound.wordpress.com/2013/02/21/how-to-compute-a-circumcircle/#more-58
            // https://en.wikipedia.org/wiki/Circumscribed_circle
            var p0 = Vertices[0];
            var p1 = Vertices[1];
            var p2 = Vertices[2];
            var dA = p0.position.X * p0.position.X + p0.position.Y * p0.position.Y;
            var dB = p1.position.X * p1.position.X + p1.position.Y * p1.position.Y;
            var dC = p2.position.X * p2.position.X + p2.position.Y * p2.position.Y;

            var aux1 = (dA * (p2.position.Y - p1.position.Y) + dB * (p0.position.Y - p2.position.Y) + dC * (p1.position.Y - p0.position.Y));
            var aux2 = -(dA * (p2.position.X - p1.position.X) + dB * (p0.position.X - p2.position.X) + dC * (p1.position.X - p0.position.X));
            var div = (2 * (p0.position.X * (p2.position.Y - p1.position.Y) + p1.position.X * (p0.position.Y - p2.position.Y) + p2.position.X * (p1.position.Y - p0.position.Y)));

            if (div == 0)
            {
                throw new DivideByZeroException();
            }

            var center = new DelaunayPoint(aux1 / div, aux2 / div);
            Circumcenter = center;
            RadiusSquared = (center.position.X - p0.position.X) * (center.position.X - p0.position.X) + (center.position.Y - p0.position.Y) * (center.position.Y - p0.position.Y);
        }

        private bool IsCounterClockwise(DelaunayPoint point1, DelaunayPoint point2, DelaunayPoint point3)
        {
            var result = (point2.position.X - point1.position.X) * (point3.position.Y - point1.position.Y) -
                (point3.position.X - point1.position.X) * (point2.position.Y - point1.position.Y);
            return result > 0;
        }

        public bool SharesEdgeWith(DelaunayTriangle triangle)
        {
            var sharedVertices = Vertices.Where(o => triangle.Vertices.Contains(o)).Count();
            return sharedVertices == 2;
        }

        public bool IsPointInsideCircumcircle(DelaunayPoint point)
        {
            var d_squared = (point.position.X - Circumcenter.position.X) * (point.position.X - Circumcenter.position.X) +
                (point.position.Y - Circumcenter.position.Y) * (point.position.Y - Circumcenter.position.Y);
            return d_squared < RadiusSquared;
        }
    }
}