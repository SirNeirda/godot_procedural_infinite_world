////////////////////////////////////////////////////////////////////////////////////////////
/// This script is part of the project "Infinite Runner", a procedural generation project
/// By Adrien Pierret
/// 
/// TerrainDetailsManager: This script takes care of the actual spawning of terrain items on a given chunk, including the water.
/// ///////////////////////////////////////////////////////////////////////////////////////

//using Bouncerock.Terrain;
using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Bouncerock.Terrain
{
    public partial class TerrainDetailsManager : Node
    {
        public static TerrainDetailsManager Instance;

        [Export]
        public string SeaWaterScene;

        public SeaWater Water;

        //public Shader WaterShader;

        //[Export] public ShaderMaterial ShaderMat;

        //[Export] string ShaderAddress;

       // [Export] string DebugShaderAddress;
        //[Export] public bool UseDebug = true;

        public override void _Ready()
        {
            Instance = this;
            PackedScene waterScene = ResourceLoader.Load<PackedScene>(SeaWaterScene);
            /*if (UseDebug)
            {
                string path = UseDebug ? DebugShaderAddress : ShaderAddress;
                WaterShader = ResourceLoader.Load<Shader>(path);
            }*/
            Water = waterScene.Instantiate() as SeaWater;
        }

        Dictionary<string, PackedScene> cachedScenes = new();

        public PackedScene GetCachedScene(string name)
        {
            string path = $"res://_scenes/decor/{name}.tscn";
            if (!cachedScenes.ContainsKey(name))
            {

                 if (!ResourceLoader.Exists(path))
                {
                    GD.PushWarning($"Scene not found at path: {path}");
                    return null;
                }
                cachedScenes[name] = ResourceLoader.Load<PackedScene>(path);
            }

            return cachedScenes[name];
        }

        public async Task<WorldItemModel> SpawnAndInitialize(WorldItem item)
        {
             if (item == null){return null;}
            //GD.Print("[Item Action] Spawning Item " + item.ModelAddress);
            if (string.IsNullOrWhiteSpace(item.ModelAddress)){return null;}
            //PackedScene Item = GetCachedScene(objectToSpawn.ObjectName);
            // GD.Print("Instantiating : " + objectToSpawn.ObjectName);
            //Here we spawn the object
                PackedScene Item = GetCachedScene(item.ModelAddress);
                if (Item != null)
            {
                item.Model = Item.Instantiate() as WorldItemModel;
            }
            
        

            return item.Model;
        }


        /*public WorldItemSettings GetSpawnedObject(string name)
        {
            foreach (WorldItemSettings obj in TerrainManager.Instance.CurrentMapSettings.NaturalObjects)
            {
                if (obj.Name == name)
                {
                    return obj;
                }
            }
            foreach (WorldItemSettings obj in TerrainManager.Instance.CurrentMapSettings.GameplayObjects)
            {
                if (obj.Name == name)
                {
                    return obj;
                }
            }
            return null;
        }*/



    }
}