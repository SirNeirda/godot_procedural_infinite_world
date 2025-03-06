using Godot;
using System;
using Bouncerock;
using GodotPlugins.Game;

public partial class PowerUp : WorldItem
{
	// Called when the node enters the scene tree for the first time.

	[Export]
	public CollisionShape3D collider;
	public  void _on_area_3d_body_entered(Node3D area)
	{
		if (area is MainCharacter)
		{
			MainCharacter chara = area as MainCharacter;
			chara.Action = chara.Action + 15;
		}
		QueueFree();
	}

}
