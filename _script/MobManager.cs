////////////////////////////////////////////////////////////////////////////////////////////
/// This script is part of the project "Infinite Runner", a procedural generation project
/// By Adrien Pierret
/// 
/// MobManager. Responsible for spreading mobs across the map regularly. If you want to disable
/// the spawn altogether, Just set SpawnPerTime to 0. 
/// ///////////////////////////////////////////////////////////////////////////////////////

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

	float timer = 0;

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

	protected void MobsUpkeep()
	{
		Vector3 charPos = GameManager.Instance.GetMainCharacterPosition();
		for (int i = Mobs.Count - 1; i >= 0; i--)
		{
			var mob = Mobs[i];
			if (!IsInstanceValid(mob)) continue;

			if (charPos.DistanceTo(mob.GlobalPosition) > 50)
			{
				DespawnMob(mob, 0);
			}
		}
	}

	//Use this to get a reasonnable spawn location. We'll be looking for a spot far enough.

	protected Vector3 GetSpawnLocation()
	{
		List<WorldItem> walls = new List<WorldItem>();
		List<float> distances = new List<float>();

		Vector3 charPos = GameManager.Instance.GetMainCharacterPosition();

		foreach (TerrainChunk chunk in TerrainManager.Instance.GetChunks(3))
		{
			if (chunk._map.GetTerrainElements() == null)
			{
				continue;
			}
			foreach (WorldItem item in chunk._map.GetTerrainElements())
			{
				if (item == null) { continue; }
				if (item.ModelAddress == "wall" && item.Model != null)
				{
					float dist = item.Model.GlobalPosition.DistanceTo(charPos);

					int index = 0;
					while (index < distances.Count && dist > distances[index])
					{
						index++;
					}

					walls.Insert(index, item);
					distances.Insert(index, dist);

					if (walls.Count > 4)
					{
						walls.RemoveAt(4);
						distances.RemoveAt(4);
					}
				}
			}
		}

		if (walls.Count == 0)
		{
			return Vector3.Zero;
		}

		int randomIndex = GD.RandRange(0, walls.Count - 1);
		WorldItem chosenWall = walls[randomIndex];

		Vector3 finalPos = new Vector3();
		finalPos.X = chosenWall.Model.GlobalPosition.X + 4;
		finalPos.Z = chosenWall.Model.GlobalPosition.Z + 4;
		finalPos.Y = TerrainManager.Instance.GetTerrainHeightAtGlobalCoordinate(
			new Vector2(finalPos.X, finalPos.Z)
		) + 2;

		return finalPos;
	}

	void SpawnMob(int number)
	{
		if (ShowHelpers)
		{
			for (int i = 0; i < number; i++)
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
		for (int i = 0; i < number; i++)
		{
			Vector3 spawnPos = GetSpawnLocation();
			if (spawnPos == Vector3.Zero) { continue; }
			CharacterMob node = Mob.Instantiate() as CharacterMob;
			AddChild(node);
			//AddChild(node);
			node.Position = spawnPos;
			node.Name = "Mob " + Mobs.Count;
			node.CharacterName = node.Name;
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
		MeshInstance3D line = LineDrawer.DrawLine3D(location, location + Vector3.Up * 500, Colors.Black);
		line.Name = "LineHelper";
		AddChild(line);
	}

}
