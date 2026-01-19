////////////////////////////////////////////////////////////////////////////////////////////
/// This script is part of the project "Infinite Runner", a procedural generation project
/// By Adrien Pierret
/// 
/// Compass: The compass shown at the top. 
/// ///////////////////////////////////////////////////////////////////////////////////////

using Godot;
using System;

public partial class Compass : Control
{
    [Export] public Label Angle;
    [Export] public PanelContainer CompassContainer;

    public float CurrentAngle = 0;

    public enum LocationToPoint { Forward, Zero }

    public override void _Process(double delta)
    {
        UpdateCompass(delta);
    }

    private float currentSmoothedAngle = 0f;
    private float velocity = 0f; // Helps simulate damping effect
    private const float damping = 10f; // Controls how quickly it settles
    private const float stiffness = 20f; // Controls how much the needle flips back and forth

    public void UpdateCompass(double delta)
    {
        float targetAngle = GetCompassHeading(GameManager.Instance.GetMainCharacter());

        // Simulating flip-flop effect with a damped spring formula
        float angleDifference = Mathf.PosMod(targetAngle - currentSmoothedAngle + 180f, 360f) - 180f; // Shortest rotation direction

        velocity += angleDifference * (float)delta * stiffness;
        velocity *= Mathf.Exp(-damping * (float)delta); // Exponential decay for damping effect
        currentSmoothedAngle += velocity * (float)delta;

        currentSmoothedAngle = Mathf.PosMod(currentSmoothedAngle, 360f);

        CallDeferred("updateAngleText");
    }

    void updateAngleText()
    {
        float invertedAngle = Mathf.PosMod(360f - currentSmoothedAngle, 360f);
        Angle.Text = Mathf.Round(invertedAngle).ToString("000");
        CompassContainer.RotationDegrees = currentSmoothedAngle;
    }

    public static float GetCompassHeading(Node3D node)
    {
        Vector3 forward = node.GlobalTransform.Basis.Z;

        float angle = Mathf.RadToDeg(Mathf.Atan2(forward.X, forward.Z));
        return Mathf.PosMod(angle + 360f, 360f); // Ensures angle is between 0-360
    }
}
