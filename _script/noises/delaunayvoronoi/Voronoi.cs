using System.Collections.Generic;

namespace Bouncerock.Procedural
{
    public class Voronoi
    {
        public IEnumerable<DelaunayEdge> GenerateEdgesFromDelaunay(IEnumerable<DelaunayTriangle> triangulation)
        {
            var voronoiEdges = new HashSet<DelaunayEdge>();
            foreach (var triangle in triangulation)
            {
                foreach (var neighbor in triangle.TrianglesWithSharedEdge)
                {
                    var edge = new DelaunayEdge(triangle.Circumcenter, neighbor.Circumcenter);
                    voronoiEdges.Add(edge);
                }
            }

            return voronoiEdges;
        }
    }
}