using Godot;
using System;
using System.Collections.Generic;

namespace Wawa.Islands
{
	public partial class EnvironmentManager : WorldEnvironment
	{
		// Declare member variables here. Examples:
		// private int a = 2;
		// private string b = "text";

		// Called when the node enters the scene tree for the first time.
		[Export]
		public Color SkyTopMainColor;
		[Export]
		public Color SkyHorizonMainColor;
		[Export]
		public Color GroundHorizonMainColor;
		[Export]
		public Color GroundBottomMainColor;
		[Export]
		public DirectionalLight3D Sunlight;


		[System.Serializable]
		public class SkySetting
		{
			public Color SkyTopMainColor;
			public Color SkyHorizonMainColor;
			public Color GroundHorizonMainColor;
			public Color GroundBottomMainColor;
		}

		public List<SkySetting> Skies = new List<SkySetting>();

		public override void _Ready()
		{
			Godot.Environment env = new Godot.Environment();
			Environment = env;
			SetSkyColor();
			ActivateFog();
		}
		public void SetSunlight()
		{
			Sunlight = new DirectionalLight3D();
			
		}
		public void SetSkyColor()
		{
			//Environment.BackgroundSky.Set("sky_top_color", color);
			GD.Print("Setting new sky color");
			ProceduralSkyMaterial newSky = new ProceduralSkyMaterial();
			newSky.SkyTopColor = SkyTopMainColor;
			newSky.SkyHorizonColor=SkyHorizonMainColor;
			newSky.GroundHorizonColor=GroundHorizonMainColor;
			newSky.GroundBottomColor=GroundBottomMainColor;

			Sky sky = new Sky();
			Environment.BackgroundMode = Godot.Environment.BGMode.Sky;
			Environment.Sky = sky;
			Environment.Sky.SkyMaterial = newSky;
			Environment.BackgroundEnergyMultiplier = 1;
		}

		public void ActivateFog()
		{
			Environment.FogEnabled = true;
			// Default values
			Environment.FogLightColor = GroundHorizonMainColor;
			Environment.FogLightEnergy = 0.5f;
			Environment.FogSunScatter = 0.4f;
			Environment.FogDensity = 0.005f;
			Environment.FogSkyAffect = 0.1f;
		}

		public void DeactivateFog()
		{
			Environment.FogEnabled = false;
		}


	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }
	}
}
