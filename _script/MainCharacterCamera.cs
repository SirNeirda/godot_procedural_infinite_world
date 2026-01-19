////////////////////////////////////////////////////////////////////////////////////////////
/// This script is part of the project "Infinite Runner", a procedural generation project
/// By Adrien Pierret
/// 
/// MainCharacterCamera: This script managing the main camera. 
/// ///////////////////////////////////////////////////////////////////////////////////////

using Bouncerock;
using Godot;
using System;

public partial class MainCharacterCamera : Camera3D
{
    [Export]
    public ColorRect SharpenEffect;
    [Export]
    public ColorRect UnderwaterEffect;
    [Export]
    public Area3D Collision;

    private int waterOverlapCount = 0;

    public void OnBodyEntered(Area3D body)
    {
        if (body is Water)
        {
            waterOverlapCount++;
            //GD.Print($"[camera] Entered water. Count = {waterOverlapCount}");

            if (waterOverlapCount == 1)
            {
                // First water overlap → actually underwater
                SetCameraUnderwater();
            }
        }
    }

    public void OnBodyExited(Node body)
    {
        if (body is Water)
        {
            waterOverlapCount = Math.Max(0, waterOverlapCount - 1);
            //GD.Print($"[camera] Exited water. Count = {waterOverlapCount}");

            if (waterOverlapCount == 0)
            {
                // No more water overlaps → above water
                SetCameraAboveWater();
            }
        }
    }

    public void SetCameraUnderwater()
    {
        SharpenEffect.Visible = false;
        UnderwaterEffect.Visible = true;
        EnvironmentManager.Instance.SunLight.ShadowEnabled = false;
        //EnvironmentManager.Instance.Environment.FogEnabled = true;
        EnvironmentManager.Instance.Environment.FogDensity = 0.01f;
        EnvironmentManager.Instance.Environment.FogAerialPerspective = 0.5f;
    }

    public void SetCameraAboveWater()
    {
        SharpenEffect.Visible = true;
        UnderwaterEffect.Visible = false;
        EnvironmentManager.Instance.SunLight.ShadowEnabled = true;
        //EnvironmentManager.Instance.Environment.FogEnabled = false;
        EnvironmentManager.Instance.Environment.FogDensity = 0.005f;
        EnvironmentManager.Instance.Environment.FogAerialPerspective = 0.779f;

    }

    public override void _Process(double delta)
    {

        if (waterOverlapCount == 1)
        {
           /* float depth = GameManager.Instance.GetMainCharacterPosition().Y;
            float currentDepthMultiplier = MathExt.InvLerp(0, -40, depth);
           float tintForce = MathExt.Lerp (0.35f, 0.70f, currentDepthMultiplier);
            ShaderMaterial shaderMat = UnderwaterEffect.Material as ShaderMaterial;
            shaderMat.SetShaderParameter("rim_color", EnvironmentManager.Instance.CurrentLightColor);*/
        }
        if (waterOverlapCount == 1)
        {
            float depth = GameManager.Instance.GetMainCharacterPosition().Y;
            float currentDepthMultiplier = MathExt.InvLerp(0, -30, depth);
           float tintForce = MathExt.Lerp (0.35f, 0.90f, currentDepthMultiplier);
           float absorbForce = MathExt.Lerp (0.6f, 0.90f, currentDepthMultiplier);
            ShaderMaterial shaderMat = UnderwaterEffect.Material as ShaderMaterial;
           // GD.Print(tintForce);
        shaderMat.SetShaderParameter("tint_strength", tintForce);
        shaderMat.SetShaderParameter("absorb_color", new Vector3(absorbForce, 0,0));
        }


    }
}
