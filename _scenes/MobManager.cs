using Bouncerock;
using Bouncerock.Terrain;
using Godot;
using System;
using Bouncerock.UI;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
public partial class MobManager : Node
{
	// Called when the node enters the scene tree for the first time.

	public static MobManager Instance;
	[Export]
	public float DelayBetweenSpawns = 3f;
	[Export]
	public int SpawnsPerTime = 4;

	[Export]
	public int MaxMobs = 20;
	[Export]
		public MeshInstance3D SafetyBubble;

	[Export]
		public bool ShowHelpers = false;

	float timer=0;

	bool GenerationActive = false;

	public Vector3 SecureZone = Vector3.Zero;

	PackedScene Mob = ResourceLoader.Load<PackedScene>("res://_scenes/mob.tscn");

	

	public List<CharacterMob> Mobs = new List<CharacterMob>();
	public override void _Ready()
	{
		//SpawnMob(SpawnsPerTime);
		Instance = this;
		Mob = ResourceLoader.Load<PackedScene>("res://_scenes/mob.tscn");
	}

	public void ResetSecureZone(Vector3 Zone)
	{
		GenerationActive = false;
		SecureZone = Zone;
		SafetyBubble.GlobalPosition = SecureZone;
	}

	public override void _Process(double delta)
	{
		timer = timer - (float)delta;
		if (timer < 0)
		{
			if (!GenerationActive)
			{
				Vector3 charPos = GameManager.Instance.GetMainCharacterPosition();

				float distance = charPos.DistanceTo(SecureZone);//the current default spawn position
				if (distance > 15)
				{
					GenerationActive = true;
				}
			}
			if (GenerationActive && MaxMobs > Mobs.Count)
			{
				SpawnMob(SpawnsPerTime);
			}
			MobsUpkeep();
			timer = DelayBetweenSpawns;
		}
		//GD.Print(timer);
	}

	protected async Task MobsUpkeep()
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
		//GD.Print("Spawning mob: " + number);
		//PackedScene Mob = ResourceLoader.Load<PackedScene>("res://_scenes/mob.tscn");
            
		//CharacterMob node = Mob.Instantiate() as CharacterMob;
		//node.Position = GetSpawnLocation();
		//CallDeferred("add_child", node);//AddChild(node);
		
		//node.Name = "Ennemy " + Mobs.Count;
		for (int i = 1; i < number; i++)
		{
           CharacterMob  node = Mob.Instantiate() as CharacterMob;
			node.Position = GetSpawnLocation();
			AddChild(node);
			node.Name = "Ennemy " + Mobs.Count;
			Mobs.Add(node);
		}

		
		
	}

	public void DespawnAllMobs()
	{
		GenerationActive = false;
		timer = DelayBetweenSpawns;
		List<CharacterMob> mobsToDespawn = new List<CharacterMob>(Mobs);
		foreach (CharacterMob mob in mobsToDespawn)
		{
			timer = DelayBetweenSpawns;
			DespawnMob(mob, 0);
		}
		timer = DelayBetweenSpawns;
		Mobs.Clear();
		
	}

	void DespawnMob(CharacterMob mob, float delay = -1)
	{
		if (mob == null)
		{
			return;
		}
		if (delay > 0)
			{
				ToSignal(GetTree().CreateTimer(delay), "timeout");
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
