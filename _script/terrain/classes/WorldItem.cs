////////////////////////////////////////////////////////////////////////////////////////////
/// This script is part of the project "Infinite Runner", a procedural generation project
/// By Adrien Pierret
/// 
/// WorldItem: The base class used for spawnable objects. If you want to crease and spawn your own object, you must add this script to it, then
/// register it inside MapSettings.cs.
/// 
/// ///////////////////////////////////////////////////////////////////////////////////////

using Godot;
using System.Collections;
using System.Collections.Generic;

namespace Bouncerock.Terrain
{

    public class WorldItem
    {


        public WorldItemSettings settings; //the settings that will determine how the variables used to spawn the object.
        [Export] public string ItemName;

        [Export] public string ModelAddress;//The name of the model.
        [Export] public Vector2 GridLocation;//Don't touch. Automatically set on spawn.

        [Export] public Vector3 WorldPosition;//Don't touch. Automatically set on spawn.
        [Export] public string Hash;

        [Export] public Vector3 Scale; //Don't touch. Automatically set on spawn.

         [Export] public Vector3 Rotation;//Don't touch. Automatically set on spawn.
        public WorldItemModel Model;//Don't touch. Automatically set on spawn.

        public virtual void Initialize(int lod)
        {
            if (Model != null)
            {
                Model.Initialize(lod);
            }
        }

        public virtual void OnChangedLOD(int lod)
        {
            if (Model != null)
            {
                Model.OnChangedLOD(lod);
            }
        }

        public void UpdateHelpers()
        {
            /*string text = meshObject.Name + " -  LOD: " + currentLODIndex + "\nCenter: " + sampleCentre + " " + IsVisible();

            if (helper == null)
            {
                helper = Debug.SetTextHelper(text, Vector3.Zero, meshObject);
                helper.MaxViewDistance = 1000;
            }
            if (helper != null)
            {
                helper.SetText(text);
            }*/
        }
    }

    //This class is intended to set basic setting for procedural generation
    public class WorldItemSettings
    {

        public enum ItemTypes {Gameplay, Static}

        public ItemTypes ItemType = ItemTypes.Gameplay;

        public float Levitation = 2;//Does the item need to be spawned slighly above ground

        public string Name;
        public string Path;
        public float MinSize = 0.9f;//Minimum scale. Determined at random before spawn.
		public float MaxSize = 1.1f;//Maximum scale. Determined at random before spawn.

        
		public bool RandomizeYRotation = true;//Straightforward

		public float RandomizeTiltAngle = 10;//How tilted the object can potentially be. 
		public float MinimumSpawnAltitude = 0;//Object will now spawn below this altitude. Zero is sea level. The vanilia version of this project goes from -200 to +200, but the sea floor is maxed out at -40 

		public float MaximumSpawnAltitude = 100;

        public float Concentration = 1f; //How many objects spawn per chunk. If under 1, it's going to be a chance to spawn on current chunk. Typically, a concentration of 1 will be seen everywhere, 0.5 will be unsurprising, and 0.01 will be a rare object and anything below will be very rare.


    }

}