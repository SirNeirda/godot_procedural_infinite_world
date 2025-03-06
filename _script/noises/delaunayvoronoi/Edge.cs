namespace Bouncerock.Procedural
{
    public class DelaunayEdge
    {
        public DelaunayPoint Point1 { get; }
        public DelaunayPoint Point2 { get; }

        public DelaunayEdge(DelaunayPoint point1, DelaunayPoint point2)
        {
            Point1 = point1;
            Point2 = point2;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            var edge = obj as DelaunayEdge;

            var samePoints = Point1 == edge.Point1 && Point2 == edge.Point2;
            var samePointsReversed = Point1 == edge.Point2 && Point2 == edge.Point1;
            return samePoints || samePointsReversed;
        }

        public override int GetHashCode()
        {
            int hCode = (int)Point1.position.X ^ (int)Point1.position.Y ^ (int)Point2.position.X ^ (int)Point2.position.Y;
            return hCode.GetHashCode();
        }
    }
}