using Bouncerock.Terrain;
using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Bouncerock
{
    public partial class TerrainDetailsManager : Node 
	{
        public static TerrainDetailsManager Instance;

        public override void _Ready()
        {
            Instance = this;
        }

        public async Task <WorldItem>SpawnAndInitialize(SpawnedObject objectToSpawn)
        {
            PackedScene Item = ResourceLoader.Load<PackedScene>("res://_scenes/decor/" + objectToSpawn.ObjectName+".tscn");
            WorldItem node = Item.Instantiate() as WorldItem;
            if (objectToSpawn is NaturalObject)
            {
                NaturalObject objectSpawned = objectToSpawn as NaturalObject;
                await Task.Run(async () =>
                {
                
                    //RandomNumberGenerator rnd = new RandomNumberGenerator();
                    //rnd.Seed = (ulong)TerrainManager.Instance.Seed;
                    
                    if (objectSpawned.RandomizeYRotation)
                    {
                        float rotation = TerrainManager.Instance.TerrainDetailsRandom.RandfRange(0,360);
                        //GD.Print("rad " + rotation);
                        rotation = Mathf.DegToRad(rotation);
                        //GD.Print("deg " + rotation);
                        //objectToSpawn.relevantItem.RotateY(rotation);
                        node.RotateY(rotation);
                    }
                    if (objectSpawned.RandomizeTiltAngle !=0)
                    {
                        
                        float rotation = TerrainManager.Instance.TerrainDetailsRandom.RandfRange(0,objectSpawned.RandomizeTiltAngle);
                        rotation = Mathf.DegToRad(rotation);
                        node.RotateZ(rotation);
                        //objectToSpawn.relevantItem.RotateZ(rotation);
                        //GD.Print(rotation);
                        float rotation2 = TerrainManager.Instance.TerrainDetailsRandom.RandfRange(0,objectSpawned.RandomizeTiltAngle);
                        rotation2 = Mathf.DegToRad(rotation2);
                        node.RotateX(rotation2);
                    // objectToSpawn.relevantItem.RotateX(rotation2);
                    }
            // CallDeferred("add_sibling",node);
                });
            }
            if (objectToSpawn is GameplayObject)
            {

            }
            
            return node;
        }


        public SpawnedObject GetSpawnedObject(string name)
		{
            foreach (SpawnedObject obj in TerrainManager.Instance.CurrentMapSettings.NaturalObjects)
            {
                if (obj.ObjectName == name)
                {
                    return obj;
                }
            }
            foreach (GameplayObject obj in TerrainManager.Instance.CurrentMapSettings.GameplayObjects)
            {
                if (obj.ObjectName == name)
                {
                    return obj;
                }
            }
            return null;
		}

        /*public WorldItem SpawnObject_deprecated(string name, Node parent = null)
        {
            //GD.Print("Loading " + name);
            PackedScene Item = ResourceLoader.Load<PackedScene>("res://_scenes/decor/" + name+".tscn");
            WorldItem node = Item.Instantiate() as WorldItem;
            
            //CallDeferred("add_sibling",node);
            if (parent != null)
            {
                parent.CallDeferred("add_child",node);
                node.CallDeferred("Initialize");
            }
            
            return node;
        }*/

        
    }
}