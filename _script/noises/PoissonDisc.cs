using System.Collections;
using System.Collections.Generic;
using Godot;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Bouncerock.Procedural
{

    public static class PoissonDiscSampling 
    {

        public static async Task<List<Vector2>> GeneratePoints(Vector2 origin, float radius, Vector2 sampleRegionSize, int samples = 10, int seed = 000) 
        {
            List<Vector2> points = new List<Vector2>();
            await Task.Run(() =>
            {
                try
                {
                    RandomNumberGenerator rand = new RandomNumberGenerator();
                    rand.Seed = (ulong)seed;
                    float cellSize = radius/Mathf.Sqrt(2);

                    int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.X/cellSize), Mathf.CeilToInt(sampleRegionSize.Y/cellSize)];
                    List<Vector2> spawnPoints = new List<Vector2>();

                    spawnPoints.Add(origin);
                    int increments = 0;
                    while (spawnPoints.Count > 0 && increments < 100) 
                    {
                        
                        int spawnIndex = rand.RandiRange(0,spawnPoints.Count-1);
                        
                        Vector2 spawnCentre = spawnPoints[spawnIndex];
                        bool candidateAccepted = false;

                        for (int i = 0; i < samples; i++)
                        {
                            float angle = rand.Randf() * Mathf.Pi * 2;
                            Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                            Vector2 candidate = spawnCentre + dir * rand.RandfRange(radius, 2*radius);
                            if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid)) 
                            {
                                points.Add(candidate);
                                spawnPoints.Add(candidate);
                                grid[(int)(candidate.X/cellSize),(int)(candidate.Y/cellSize)] = points.Count;
                                candidateAccepted = true;
                                break;
                            }
                        }
                        if (!candidateAccepted) 
                        {
                            spawnPoints.RemoveAt(spawnIndex);
                        }
                        increments++;
                    }
                }
                catch (Exception ex)
                {
                    GD.Print("Error in poisson sampling: " + ex.StackTrace);
                }
            });
            return points;
        }

        public static async Task<List<Vector2>> Test(Vector2 size, Vector2 center, int generatePoints = 2) 
        {
            List<Vector2> points = new List<Vector2>();
            RandomNumberGenerator rand = new RandomNumberGenerator();
            
            await Task.Run(() =>
			{

            for (int i = 0; i < generatePoints; i++)
            {
                rand.Seed = (ulong)i;
                points.Add(new Vector2(rand.RandfRange(0, size.X), rand.RandfRange(0, size.Y)));
                //GD.Print(new Vector2(rand.RandfRange(0, size.X), rand.RandfRange(0, size.Y)));
            }
            });

            return points;
        }

        public static List<DelaunayPoint> ToDelaunayPoints(List<Vector2> points)
        {
            var delpoints = new List<DelaunayPoint>();
            foreach (Vector2 point in points)
            {
                delpoints.Add(new DelaunayPoint(point.X, point.Y));
            }
            return delpoints;
        }

        static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid) 
        {

            if (candidate.X >=0 && candidate.X < sampleRegionSize.X && candidate.Y >= 0 && candidate.Y < sampleRegionSize.Y) 
            {
                int cellX = (int)(candidate.X/cellSize);
                int cellY = (int)(candidate.Y/cellSize);
                int searchStartX = Mathf.Max(0,cellX -2);
                int searchEndX = Mathf.Min(cellX+2,grid.GetLength(0)-1);
                int searchStartY = Mathf.Max(0,cellY -2);
                int searchEndY = Mathf.Min(cellY+2,grid.GetLength(1)-1);

                for (int x = searchStartX; x <= searchEndX; x++) 
                {
                    for (int y = searchStartY; y <= searchEndY; y++) 
                    {
                        int pointIndex = grid[x,y]-1;
                        if (pointIndex != -1) 
                        {
                            //float sqrDst = (candidate - points[pointIndex]).sqrMagnitude; 
                            float sqrDst = MathExt.SqrMag(candidate - points[pointIndex]);
                            if (sqrDst < radius*radius) 
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}