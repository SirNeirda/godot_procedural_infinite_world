using Bouncerock;
using Bouncerock.Terrain;
using Godot;
using System;
using Bouncerock.UI;
using System.Threading;
using System.Collections.Generic;
public partial class MobManager : Node
{
	// Called when the node enters the scene tree for the first time.

	[Export]
	public float DelayBetweenSpawns = 3f;
	[Export]
	public int SpawnsPerTime = 4;

	[Export]
	public int MaxMobs = 40;

	[Export]
		public bool ShowHelpers = false;

	float timer=0;

	bool GenerationActive = false;

	

	public List<CharacterMob> Mobs = new List<CharacterMob>();
	public override void _Ready()
	{
		//SpawnMob(SpawnsPerTime);
	}

	public override void _Process(double delta)
	{
		timer = timer-(float)delta;
		if (timer < 0)
		{
			if (!GenerationActive)
			{
				Vector3 charPos = GameManager.Instance.GetMainCharacterPosition();
				float distance = charPos.DistanceTo(Vector3.Zero);
				if (distance >10)
				{
					GenerationActive = true;
				}
			}
			if (GenerationActive && MaxMobs > Mobs.Count)
			{
				SpawnMob(SpawnsPerTime);
				MobsUpkeep();
			}
			timer = DelayBetweenSpawns;
		}
		//GD.Print(timer);
	}

	protected void MobsUpkeep()
	{
		foreach (CharacterMob mob in Mobs)
		{
			Vector3 charPos = GameManager.Instance.GetMainCharacterPosition();
			float distance = charPos.DistanceTo(mob.Position);
			if (distance >50)
			{
				DespawnMob(mob, 2);
			}
		}
	}

	//Use this to get a reasonnable spawn location. We'll be looking for a spot far enough.

	protected Vector3 GetSpawnLocation()
	{
		Random random = new Random();

		Vector3 charPos = GameManager.Instance.GetMainCharacterPosition();

		// Generate a random angle between 0 and 2 * PI
		float angle = (float)(random.NextDouble() * 2 * Math.PI);

		// Generate a random distance between minDistance and maxDistance
		float distance = (float)(random.NextDouble() * (10 - 50) + 10);

		// Calculate the offset using polar coordinates
		float offsetX = distance * (float)Math.Cos(angle);
		float offsetZ = distance * (float)Math.Sin(angle);

		Vector3 finalPos = new Vector3
		{
			X = charPos.X + offsetX,
			Z = charPos.Z + offsetZ
		};

		// Set the Y coordinate based on the terrain height, plus some offset (e.g., +5 units)
		finalPos.Y = TerrainManager.Instance.GetTerrainHeightAtGlobalCoordinate(new Vector2(finalPos.X, finalPos.Z)) + 5;

		return finalPos;
	}

	void SpawnMob(int number)
	{
		if (ShowHelpers)
		{
			for (int i = 1; i < number; i++)
			{
				SetHelper(GetSpawnLocation());
				return;
			}

		}
		GD.Print("Spawning mob: " + number);
		PackedScene Mob = ResourceLoader.Load<PackedScene>("res://_scenes/mob.tscn");
            
		CharacterMob node = Mob.Instantiate() as CharacterMob;
		node.Position = GetSpawnLocation();
		AddChild(node);
		Mobs.Add(node);
		node.Name = "Ennemy " + Mobs.Count;
		for (int i = 1; i < number; i++)
		{
            node = Mob.Instantiate() as CharacterMob;
			node.Position = GetSpawnLocation();
			AddChild(node);
			node.Name = "Ennemy " + Mobs.Count;
		}

		
		
	}

	async void DespawnMob(CharacterMob mob, float delay = -1)
	{
        if (delay > 0)
        {
            await ToSignal(GetTree().CreateTimer(delay), "timeout");
        }
        
        mob.QueueFree();
		Mobs.Remove(mob);
	}

	public void SetHelper(Vector3 location)
	{
			MeshInstance3D line = LineDrawer.DrawLine3D(location, location+Vector3.Up*500, Colors.Black);
			line.Name = "LineHelper";
			AddChild(line);
	}

}
