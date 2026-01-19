using Godot;
using System;
using Bouncerock.Terrain;
using GodotPlugins.Game;

public partial class Tower : WorldItemModel
{
    [Export]
    public CollisionShape3D collider;

    private Area3D colliderArea;

    public override void _Ready()
    {
        // Find the parent Area3D of the collider
        colliderArea = collider.GetParent() as Area3D;
        if (colliderArea == null)
        {
            GD.PrintErr("Collider must be a child of an Area3D!");
            return;
        }

        // Connect signals
        colliderArea.BodyEntered += OnBodyEntered;
        colliderArea.BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is MainCharacter)
        {
            MainCharacter charbody = body as MainCharacter;
            charbody.FloorMaxAngle = 80;
            MobManager.Instance.DespawnAllMobs();
            MobManager.Instance.SpawnsPerTime = 0;
            GD.Print("Entered tower area");
        }
        else if (body.GetParent() is WorldItemModel)
        {
           // GD.Print("body is " + body.GetParent().Name);
            body.Visible = false;
        }
    }

    private void OnBodyExited(Node3D body)
    {
        if (body is MainCharacter)
        {
            MainCharacter charbody = body as MainCharacter;
            charbody.FloorMaxAngle = 60;
            MobManager.Instance.SpawnsPerTime = 4;
            GD.Print("Exited tower area");
        }
    }
}
