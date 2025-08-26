/*MIT License

Copyright (c) 2025 Adrien Pierret

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.*/



/// <summary>
/// Compass manager
/// This script provides a compass to your scene, pointing either at forward, or at the zero position.
/// </summary>
/// 

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
