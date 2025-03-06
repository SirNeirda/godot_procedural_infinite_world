using Godot;

namespace Bouncerock 
{ 
    
    public static class MathExt
    {
        public static float Lerp(float firstFloat, float secondFloat, float by)
		{
			return firstFloat * (1 - by) + secondFloat * by;
		}
		
        public static float InvLerp(float minValue, float maxValue, float currentValue)
        {
            return (currentValue-minValue) / (maxValue-minValue);
        }

        public static float Clamp01(float value)
        {
            return Mathf.Clamp(value, 0, 1);
        }

        public static Vector3 BoundingMin3 (Aabb bound)
        {
           return bound.GetCenter() + (bound.Size/2);
        }

        public static float SqrDistance(Vector3 point, Aabb bound)
        {
            /*Vector3 max = BoundingMax3(bound);
            Vector3 min = BoundingMin3(bound);
            Vector3 pointOnBounds;
            pointOnBounds.x = Mathf.Clamp(point.x, min.x, max.x);
            pointOnBounds.y = Mathf.Clamp(point.y, min.y, max.y);
            pointOnBounds.z = Mathf.Clamp(point.z, min.z, max.z);
            Vector3 dist = pointOnBounds - point;
            return dist.Dot(dist);*/
            Vector3 closestPoint = new Vector3();
            closestPoint.X = Mathf.Clamp(point.X, bound.Position.X, bound.End.X);
            closestPoint.Y = Mathf.Clamp(point.Y, bound.Position.Y, bound.End.Y);
            closestPoint.Z = Mathf.Clamp(point.Z, bound.Position.Z, bound.End.Z);

            return (point - closestPoint).LengthSquared();
        }

        public static float SqrDistance2(Vector2 point, Rect2 bound)
        {
            /*Vector2 max = BoundingMax2(bound);
            Vector2 min = BoundingMin2(bound);
            Vector2 pointOnBounds;
            pointOnBounds.x = Mathf.Clamp(point.x, min.x, max.x);
            pointOnBounds.y = Mathf.Clamp(point.y, min.y, max.y);
            Vector2 dist = pointOnBounds - point;
            return dist.Dot(dist);*/
            Vector2 closestPoint = new Vector2();
            closestPoint.X = Mathf.Clamp(point.X, bound.Position.X, bound.End.X);
            closestPoint.Y = Mathf.Clamp(point.Y, bound.Position.Y, bound.End.Y);

            return (point - closestPoint).LengthSquared();
        }

        public static float SqrMag(Vector2 point)
        {
            return Mathf.Pow(point.X,2) + Mathf.Pow(point.Y,2);
        }

        public static bool IsVector2InAABB(Vector2 vec, Aabb aabb)
            {
                //In Godot, position is min and end is max
                return vec.X >= aabb.Position.X && vec.X <= aabb.End.X && vec.Y >= aabb.Position.Y && vec.Y <= aabb.End.Y;
            }

         public static bool IsVector2InUpperLeftCorner(Vector2 vec, Vector2 chunkCenter)
            {
                //return vec.X <= aabb.Position.X && vec.Y >= aabb.End.Y;
                 return vec.X <= chunkCenter.X && vec.Y >= chunkCenter.Y;
            }
    
        public static bool IsVector2InUpperRightCorner(Vector2 vec, Vector2 chunkCenter)
            {
               // return vec.X >= aabb.End.X && vec.Y >= aabb.End.Y;
               return vec.X >= chunkCenter.X && vec.Y >= chunkCenter.Y;
            }
        
        public static bool IsVector2InLowerLeftCorner(Vector2 vec, Vector2 chunkCenter)
            {
                return vec.X <= chunkCenter.X && vec.Y <= chunkCenter.Y;
            }
        
        public static bool IsVector2InLowerRightCorner(Vector2 vec, Vector2 chunkCenter)
            {
                return vec.X >= chunkCenter.X && vec.Y <= chunkCenter.Y;
            }

        public static Vector2 GetCurrentHalfBoundPosition(Vector2 vec, Aabb bound)
        {
            Vector2 chunkCenter = new Vector2 (bound.Position.X, bound.Position.Z);
            if (IsVector2InUpperLeftCorner(vec, chunkCenter)) {return new Vector2(-1,1);}
            if (IsVector2InUpperRightCorner(vec, chunkCenter)) {return new Vector2(1,1);}
            if (IsVector2InLowerLeftCorner(vec, chunkCenter)) {return new Vector2(-1,-1);}
            if (IsVector2InLowerRightCorner(vec, chunkCenter)) {return new Vector2(1,-1);}
            return Vector2.Zero;
        }

        public static float CalculateInclination(Vector3 coord1, Vector3 coord2,Vector3 coord3,Vector3 coord4)
        {
            //new Vector3(0, 0, 0),
            //new Vector3(1, 0, 0),
            //new Vector3(1, 1, 0),
           // new Vector3(0, 1, 0)
        
            Vector3 BA = coord1 - coord2;
            Vector3 BC = coord3 - coord4;
            
            Vector3 normal = BA.Cross(BC);
            float inclination = (float)Mathf.Acos(normal.Dot(Vector3.Back) / normal.Length()) * 180 / Mathf.Pi;
            return inclination;
        }

    }
}