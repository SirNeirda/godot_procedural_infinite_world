using Bouncerock.Terrain;
using Godot;
using System.Collections;
using System.Collections.Generic;

namespace Bouncerock
{
    
    public partial class WorldItem : Node3D 
	{
       /* [Export]
       public string Path;
        [Export]
        public float MaxSize;
        [Export]
        public float MinSize;
        [Export]
        public bool RandomizeYRotation = true;

        [Export]
        //In degrees, from 0 to 90;
        public float RandomizeTiltAngle = 10;

        [Export]
        public float MinimumSpawnAltitude = 0;

        
        [Export]
        public float MaximumSpawnAltitude = 100;

        public WorldItemSettings BaseSettings = new WorldItemSettings();
        public WorldItemData BaseData= new WorldItemData();

        public void Initialize()
        {
            //RandomNumberGenerator rnd = new RandomNumberGenerator();
            //rnd.Seed = (ulong)TerrainManager.Instance.Seed;

            if (RandomizeYRotation)
            {
                float rotation = TerrainManager.Instance.TerrainDetailsRandom.RandfRange(0,360);
                //GD.Print("rad " + rotation);
                rotation = Mathf.DegToRad(rotation);
                //GD.Print("deg " + rotation);
                RotateY(rotation);
            }
             if (RandomizeTiltAngle !=0)
            {
                
                float rotation = TerrainManager.Instance.TerrainDetailsRandom.RandfRange(0,RandomizeTiltAngle);
                rotation = Mathf.DegToRad(rotation);
                RotateZ(rotation);
                //GD.Print(rotation);
                float rotation2 = TerrainManager.Instance.TerrainDetailsRandom.RandfRange(0,RandomizeTiltAngle);
                rotation2 = Mathf.DegToRad(rotation2);
                RotateX(rotation2);
            }
        }*/
    }
}