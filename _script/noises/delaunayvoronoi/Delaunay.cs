using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using System.Threading.Tasks;

namespace Bouncerock.Procedural
{
    public class DelaunayTriangulator
    {
        private float MaxX { get; set; }
        private float MaxY { get; set; }
        private IEnumerable<DelaunayTriangle> border;

        public IEnumerable<DelaunayPoint> GeneratePoints(int amount, Vector2 location, float maxX, float maxY)
        {
            MaxX = maxX;
            MaxY = maxY;
            

            // TODO make more beautiful
            var point0 = new DelaunayPoint(0, 0);
            var point1 = new DelaunayPoint(0, MaxY);
            var point2 = new DelaunayPoint(MaxX, MaxY);
            var point3 = new DelaunayPoint(MaxX, 0);
            var points = new List<DelaunayPoint>() { point0, point1, point2, point3 };
            var tri1 = new DelaunayTriangle(point0, point1, point2);
            var tri2 = new DelaunayTriangle(point0, point2, point3);
            border = new List<DelaunayTriangle>() { tri1, tri2 };


            RandomNumberGenerator numgen = new RandomNumberGenerator();
          
            numgen.Seed = ((ulong)location.GetHashCode()); //Convert.ToUInt64(location.ToString());//((ulong)location.GetHashCode());
            //numgen.State = 0;
            for (int i = 0; i < amount - 4; i++)
            {
                //numgen.State = numgen.State+1;
               
                var pointX = numgen.Randf() * MaxX;
                var pointY = numgen.Randf() * MaxY;
                points.Add(new DelaunayPoint(pointX, pointY));
                // GD.Print("State: " + numgen.State + " - Position"+ pointX + "/" + pointY);
            }

            return points;
        }

        public async Task<IEnumerable<DelaunayPoint>> GeneratePointsPoisson(int amount, int size)
        {
            
            // TODO make more beautiful
            var point0 = new DelaunayPoint(0, 0);
            var point1 = new DelaunayPoint(0, size);
            var point2 = new DelaunayPoint(size, size);
            var point3 = new DelaunayPoint(size, 0);

            var points = new List<DelaunayPoint>();
            border = new List<DelaunayTriangle>();

            Vector2 framesize = new Vector2(size,size);
            Vector2 origin = framesize/2;
            

            List<Vector2> disc = await PoissonDiscSampling.GeneratePoints(origin, amount,framesize,30);
            foreach (Vector2 point in disc)
            {
                points.Add(new DelaunayPoint((float)point.X, (float)point.Y));
            }
            return points;
        }

        public IEnumerable<DelaunayTriangle> BowyerWatson(IEnumerable<DelaunayPoint> points)
        {
            var supraTriangle = GenerateSupraTriangle();
            var triangulation = new HashSet<DelaunayTriangle>(border);

            foreach (var point in points)
            {
                var badTriangles = FindBadTriangles(point, triangulation);
                var polygon = FindHoleBoundaries(badTriangles);

                foreach (var triangle in badTriangles)
                {
                    foreach (var vertex in triangle.Vertices)
                    {
                        vertex.AdjacentTriangles.Remove(triangle);
                    }
                }
                triangulation.RemoveWhere(o => badTriangles.Contains(o));

                foreach (var edge in polygon.Where(possibleEdge => possibleEdge.Point1 != point && possibleEdge.Point2 != point))
                {
                    var triangle = new DelaunayTriangle(point, edge.Point1, edge.Point2);
                    triangulation.Add(triangle);
                }
            }

            triangulation.RemoveWhere(o => o.Vertices.Any(v => supraTriangle.Vertices.Contains(v)));
            return triangulation;
        }

        private List<DelaunayEdge> FindHoleBoundaries(ISet<DelaunayTriangle> badTriangles)
        {
            var edges = new List<DelaunayEdge>();
            foreach (var triangle in badTriangles)
            {
                edges.Add(new DelaunayEdge(triangle.Vertices[0], triangle.Vertices[1]));
                edges.Add(new DelaunayEdge(triangle.Vertices[1], triangle.Vertices[2]));
                edges.Add(new DelaunayEdge(triangle.Vertices[2], triangle.Vertices[0]));
            }
            var grouped = edges.GroupBy(o => o);
            var boundaryEdges = edges.GroupBy(o => o).Where(o => o.Count() == 1).Select(o => o.First());
            return boundaryEdges.ToList();
        }
        public IEnumerable<DelaunayPoint> GenerateFromVectors(List<Vector2> vectors)
        {
             MaxX = 240;
            MaxY = 240;

            // TODO make more beautiful
            var point0 = new DelaunayPoint(0, 0);
            var point1 = new DelaunayPoint(0, MaxY);
            var point2 = new DelaunayPoint(MaxX, MaxY);
            var point3 = new DelaunayPoint(MaxX, 0);
            var points = new List<DelaunayPoint>() { point0, point1, point2, point3 };
            var tri1 = new DelaunayTriangle(point0, point1, point2);
            var tri2 = new DelaunayTriangle(point0, point2, point3);
            border = new List<DelaunayTriangle>() { tri1, tri2 };
            
            foreach (Vector2 point in vectors)
            {
                points.Add(new DelaunayPoint(point.X, point.Y));
            }
            return points;
        }

        private DelaunayTriangle GenerateSupraTriangle()
        {
            //   1  -> maxX
            //  / \
            // 2---3
            // |
            // v maxY
            var margin = 500;
            var point1 = new DelaunayPoint(0.5f * MaxX, -2 * MaxX - margin);
            var point2 = new DelaunayPoint(-2 * MaxY - margin, 2 * MaxY + margin);
            var point3 = new DelaunayPoint(2 * MaxX + MaxY + margin, 2 * MaxY + margin);
            return new DelaunayTriangle(point1, point2, point3);
        }

        private ISet<DelaunayTriangle> FindBadTriangles(DelaunayPoint point, HashSet<DelaunayTriangle> triangles)
        {
            var badTriangles = triangles.Where(o => o.IsPointInsideCircumcircle(point));
            return new HashSet<DelaunayTriangle>(badTriangles);
        }

        
    }
}