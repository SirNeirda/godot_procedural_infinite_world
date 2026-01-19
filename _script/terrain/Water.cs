////////////////////////////////////////////////////////////////////////////////////////////
/// This script is part of the project "Infinite Runner", a procedural generation project
/// By Adrien Pierret
/// 
/// Water: This script mainly manages the buyoancy. Buyoancy is the "floatation" of objects at the surface of the water.
/// For now, the only buyoant object is the boxes you can spawn with mouse click. This is an extremely simplified calculation
/// of buyancy. Proper implementation would depend on the type of game.
/// ///////////////////////////////////////////////////////////////////////////////////////

using Bouncerock;
using Godot;
using GodotPlugins.Game;
using System.Collections.Generic;

public partial class Water : Area3D
{
    [Export] public float BuoyancyStrength = 25f;
    [Export] public float Drag = 3f;
    [Export] public float UprightForce = 10f;

    [Export] public float Offset = -2f;


    private HashSet<RigidBody3D> bodiesInWater = new();
    private float waterSurfaceY;



    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        var shapeNode = GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
        if (shapeNode != null && shapeNode.Shape is BoxShape3D box)
        {
            // CollisionShape3D scale, not the Water node
            float worldScaleY = shapeNode.GlobalTransform.Basis.Scale.Y;

            waterSurfaceY = shapeNode.GlobalPosition.Y
                            + (box.Size.Y * worldScaleY) * 0.5f
                            + Offset;

            //GD.Print($"Surface: {waterSurfaceY}");
        }
        else
        {
            waterSurfaceY = GlobalPosition.Y + Offset;
        }
    }




    private void OnBodyEntered(Node body)
    {
        if (body is RigidBody3D rb)
        {
            bodiesInWater.Add(rb);
            // GD.Print($"[Water] {rb.Name} entered water.");
        }
        if (body is CollisionShape3D area)
        {
            // GD.Print($"[Water] {area.Name} entered water.");
            if (area.GetParent() is MainCharacterCamera)
            {
                MainCharacterCamera cam = area.GetParent() as MainCharacterCamera;
                cam.OnBodyEntered(this);
            }
        }

    }

    private void OnBodyExited(Node body)
    {
        if (body is RigidBody3D rb)
        {
            bodiesInWater.Remove(rb);
            //GD.Print($"[Water] {rb.Name} exited water.");
        }
        if (body is Area3D area)
        {
            if (area.GetParent() is MainCharacterCamera)
            {
                MainCharacterCamera cam = area.GetParent() as MainCharacterCamera;
                cam.OnBodyExited(this);
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (var rb in bodiesInWater)
        {
            if (rb == null || !IsInstanceValid(rb))
                continue;

            float depth = waterSurfaceY - rb.GlobalPosition.Y;
            // Check if submerged
            if (depth > 0.5f)
            {
                // Smoothly calculate how "submerged" the object is
                float submersion = MathExt.Clamp01(depth + 0.5f); // soft transition
                float buoyantFactor = submersion * submersion;  // ease in

                // fake buoyancy
                float verticalVel = rb.LinearVelocity.Y;
                // float targetYVel = -verticalVel * 0.8f; 
                Vector3 buoyantForce = Vector3.Up * (BuoyancyStrength * buoyantFactor - verticalVel * 5f);

                //  Horizontal drag 
                Vector3 horizontalVel = rb.LinearVelocity;
                horizontalVel.Y = 0;
                Vector3 waterDrag = -horizontalVel * (Drag * buoyantFactor * 2f);

                //  Apply forces 
                rb.ApplyCentralForce((buoyantForce + waterDrag) * (float)delta * 60f); // scaled for frame rate

                // Soft upright correction 
                Basis basis = rb.GlobalTransform.Basis;
                Vector3 upDir = basis.Y.Normalized();
                Vector3 tilt = upDir.Cross(Vector3.Up);
                rb.ApplyTorque(-tilt * UprightForce * submersion);

                // Extra damping under water
                rb.LinearDamp = 2f;
                rb.AngularDamp = 2f;
            }
            else
            {
                // Reset to light damping above water
                rb.LinearDamp = 0.05f;
                rb.AngularDamp = 0.05f;
            }
        }
    }
}
