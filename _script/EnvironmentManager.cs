////////////////////////////////////////////////////////////////////////////////////////////
/// This script is part of the project "Infinite Runner", a procedural generation project
/// By Adrien Pierret
/// 
/// GameManager: This script modifies the skybox depending on the time of the day.
/// It also manages the presence of clouds, with several presets, all determined at random at midnight each day.
/// Note that it can be use completely independently to the project. But you'll need a Skybox.
/// 
/// ///////////////////////////////////////////////////////////////////////////////////////

using Bouncerock;
using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// a full day/night cycle 
/// </summary>
public partial class EnvironmentManager : WorldEnvironment
{
    public static EnvironmentManager Instance;
    [Export] public DirectionalLight3D SunLight;
    [Export] public float DayLengthInMinutes = 60f; //24-hour cycle

    [Export] public float StartAtHour = 14f;

    [Export] public bool RandomStart = true;

    [ExportCategory("Cloud Settings")]

    [Export] public Gradient SpottyGradient = new Gradient();

    [Export] public Gradient CloudyGradient = new Gradient();

    [Export] public Gradient CoveredGradient = new Gradient();

    [Export] public Gradient SunnyGradient = new Gradient();

    public enum CloudSettings { Clear, Spotty, Cloudy, Covered }

    [Export] public CloudSettings CurrentCloud = CloudSettings.Clear;

    public Gradient CurrentGradient;

    private Gradient previousGradient;
    private float gradientLerpValue = 1.0f;
    [Export] public float GradientBlendDuration = 10f; // seconds to blend

    [ExportCategory("Dawn Colors")]
    [Export] public Color AM_DaytimeSunTop = new Color(0.4f, 0.6f, 0.8f);
    [Export] public Color AM_DaytimeSunGround = new Color(0.9f, 0.7f, 0.5f);
    [Export] public Color AM_DaytimeLightColor = new Color(1.0f, 0.8f, 0.6f);

    [Export] public Color AM_CloudColor = new Color(1.0f, 0.9f, 0.75f);

    [Export] public Color AM_FogColor = new Color(1.0f, 0.9f, 0.75f);
    [Export] public float AM_DaytimeLightEnergy = 0.8f;

    [Export] public float AM_TimeStart = 5f; //Starts at 5AM
    [Export] public float AM_TimeEnd = 7f;//Ends at 7AM

    [Export] public float AM_TransitionTimeToMorning = 0.5f; //Transitions to morning in half an hour

    [ExportCategory("Morning Colors")]
    [Export] public Color NOON_DaytimeSunTop = new Color(0.5f, 0.75f, 1.0f);
    [Export] public Color NOON_DaytimeSunGround = new Color(0.7f, 0.9f, 1.0f);
    [Export] public Color NOON_DaytimeLightColor = new Color(1.0f, 1.0f, 0.95f);

    [Export] public Color NOON_CloudColor = new Color(1.0f, 0.9f, 0.75f);

    [Export] public Color NOON_FogColor = new Color(1.0f, 0.9f, 0.75f);
    [Export] public float NOON_DaytimeLightEnergy = 1.5f;

    [Export] public float NOON_TimeStart = 7f;
    [Export] public float NOON_TimeEnd = 12f;

    [Export] public float AM_TransitionTimeToAfternoon = 2f; //Transitions to PM in 2h

    [ExportCategory("Afternoon Colors")]
    [Export] public Color PM_DaytimeSunTop = new Color(0.6f, 0.7f, 0.95f);
    [Export] public Color PM_DaytimeSunGround = new Color(0.9f, 0.7f, 0.6f);
    [Export] public Color PM_DaytimeLightColor = new Color(1.0f, 0.9f, 0.75f);
    [Export] public Color PM_CloudColor = new Color(1.0f, 0.9f, 0.75f);

    [Export] public Color PM_FogColor = new Color(1.0f, 0.9f, 0.75f);
    [Export] public float PM_DaytimeLightEnergy = 1.2f;

    [Export] public float PM_TimeStart = 12f;
    [Export] public float PM_TimeEnd = 18f;
    [Export] public float AM_TransitionTimeToDusk = 1f; //Transitions to PM in 1h

    [ExportCategory("Dusk Colors")]
    [Export] public Color EV_DaytimeSunTop = new Color(0.3f, 0.2f, 0.5f);
    [Export] public Color EV_DaytimeSunGround = new Color(1.0f, 0.5f, 0.3f);
    [Export] public Color EV_DaytimeLightColor = new Color(1.0f, 0.6f, 0.4f);

    [Export] public Color EV_CloudColor = new Color(1.0f, 0.9f, 0.75f);

    [Export] public Color EV_FogColor = new Color(1.0f, 0.9f, 0.75f);
    [Export] public float EV_DaytimeLightEnergy = 0.6f;

    [Export] public float EV_TimeStart = 18f;
    [Export] public float EV_TimeEnd = 21f;
    [Export] public float EV_TransitionTimeToNight = 1f; //Transitions to PM in 1h

    [ExportCategory("Night Colors")]
    [Export] public Color Night_SunTop = new Color(0.05f, 0.05f, 0.1f);
    [Export] public Color Night_Ground = new Color(0.1f, 0.1f, 0.15f);
    [Export] public Color Night_LightColor = new Color(0.3f, 0.35f, 0.4f);
    [Export] public Color Night_CloudColor = new Color(1.0f, 0.9f, 0.75f);

    [Export] public Color Night_FogColor = new Color(1.0f, 0.9f, 0.75f);
    [Export] public float Night_LightEnergy = 0.1f;

    [Export] public float Night_TimeStart = 21f;
    [Export] public float Night_TimeEnd = 7f;

    [Export] public float AM_TransitionTimeToDawn = 1f; //Transitions to dawn in an hour

    [ExportCategory("Shadow Settings")]
    [Export] public bool EnableShadowControl = true;

    [ExportCategory("Speed Multiplier")]

    [Export] public float CycleSpeed = 100;

    [Export] public float CloudsSpeed = 1;

    private float currentTime = 12f; // In-game hour [0.0, 24.0]
    private float secondsPerHour;
    //private bool shadowsEnabled = true;

    public Color CurrentLightColor;

    public Color CurrentCloudColor;

    public Color CurrentFogColor;
    private ProceduralSkyMaterial skyMaterial;

    //private readonly TransitionPhase dawn = new TransitionPhase { StartHour = 4.5f, EndHour = 7.0f };
    //private readonly TransitionPhase dusk = new TransitionPhase { StartHour = 18.5f, EndHour = 20.5f };


    public override void _Ready()
    {
        secondsPerHour = (DayLengthInMinutes * 60f) / 24f;

        Instance = this;
        CurrentGradient = SunnyGradient;
        DetermineWeather();


        // Set or create ProceduralSkyMaterial
        skyMaterial = Environment.Sky?.SkyMaterial as ProceduralSkyMaterial;
        if (skyMaterial == null)
        {
            skyMaterial = new ProceduralSkyMaterial();
            if (Environment.Sky != null)
            {
                Environment.Sky.SkyMaterial = skyMaterial;
            }
            else
            {
                GD.PrintErr("WorldEnvironment has no sky assigned.");
            }
        }
        currentTime = StartAtHour;
        if (StartAtHour < 0 || RandomStart)
        {
            currentTime = GetRandomTime();
        }
        UpdateLighting();
    }

    public float GetRandomTime()
    {
        return (float)GD.RandRange(0.0, 24.0);
    }

    public string GetTime()
    {
        return currentTime.ToString();
    }

    public override void _Process(double delta)
    {
        currentTime += ((float)delta / secondsPerHour) * CycleSpeed;
        if (currentTime >= 24f)
        {
            currentTime -= 24f;
            DetermineWeather();
        }

        UpdateLighting();
        UpdateClouds(delta);
    }
private void ResetGradient(Gradient g)
{
    // Replace default two points by 1 dummy point
    // (minimum allowed by Godot)
    g.SetOffset(0, 0f); 
    g.SetColor(0, Colors.White);

    // If gradient had more than 1 point, trim extra from end
    for (int i = g.GetPointCount() - 1; i > 0; i--)
    {
        g.RemovePoint(i);
    }
}
   private Gradient InterpolateGradients(Gradient grad1, Gradient grad2, float t)
{
    if (grad1 == null || grad2 == null)
    {
        return new Gradient();
    }

    t = Mathf.Clamp(t, 0f, 1f);

    Gradient output = new Gradient();
    //ResetGradient(output); // Safe clear

    // Collect all unique offsets
    List<float> offsets = new List<float>();

    for (int i = 0; i < grad1.GetPointCount(); i++)
    {
        offsets.Add(grad1.GetOffset(i));
    }

    for (int i = 0; i < grad2.GetPointCount(); i++)
    {
        float o = grad2.GetOffset(i);
        if (!offsets.Contains(o))
        {
            offsets.Add(o);
        }
    }

    offsets.Sort();


    foreach (float pos in offsets)
    {
        Color c1 = grad1.Sample(pos);
        Color c2 = grad2.Sample(pos);
        Color blended = c1.Lerp(c2, t);

        output.AddPoint(pos, blended);
    }

    return output;
}

    private void DetermineWeather()
    {
       Array values = Enum.GetValues(typeof(CloudSettings));
    CloudSettings newWeather = (CloudSettings)values.GetValue(GD.Randi() % values.Length);

    if (CurrentCloud == newWeather)
        return;

    // Store previous
    previousGradient = CurrentGradient;

    // Set new cloud type + target gradient
    CurrentCloud = newWeather;

    switch (CurrentCloud)
    {
        case CloudSettings.Clear:   CurrentGradient = SunnyGradient; break;
        case CloudSettings.Spotty:  CurrentGradient = SpottyGradient; break;
        case CloudSettings.Cloudy:  CurrentGradient = CloudyGradient; break;
        case CloudSettings.Covered: CurrentGradient = CoveredGradient; break;
    }

    // Start blending from 0 → 1
    gradientLerpValue = 0f;

    GD.Print("New clouds: " + CurrentCloud);
    }

    private void UpdateClouds(double delta)
    {
        
        if (skyMaterial != null)
        {
            NoiseTexture2D noiseTexture = skyMaterial.SkyCover as NoiseTexture2D;
            FastNoiseLite noiseLite = noiseTexture.Noise as FastNoiseLite;

            float normalizedTOD = MathExt.InvLerp(0, 24, currentTime);
            float timeOfDay = MathExt.Lerp(0, 360, normalizedTOD * CloudsSpeed);
            Vector3 offset = new Vector3(timeOfDay, 0, 0);
            noiseLite.Offset = offset;
            // Blending
    if (previousGradient != null && CurrentGradient != null && gradientLerpValue < 1f)
    {
        gradientLerpValue += (float)delta / GradientBlendDuration;

        float t = Mathf.Clamp(gradientLerpValue, 0f, 1f);

        Gradient grad = InterpolateGradients(previousGradient, CurrentGradient, t);

        noiseTexture.ColorRamp = grad;

       // GD.Print($"Blending: {t}");
    }
        }
    }

    /// Updates sun angle, sky color, and light based on current in-game time.
    private void UpdateLighting()
    {
        float timeNormalized = currentTime / 24f; //
                                                  // float eased = 0.5f - 0.5f * Mathf.Cos(timeNormalized * Mathf.Pi * 2f); // cosine ease
        float hour = currentTime;
        float sunAngle = Mathf.Lerp(-90f, 270f, timeNormalized); //use eased so that the sun doesn't go below the horizon line, use timeNormalized otherwise

        // simulate sun movement
        if (SunLight != null)
        {
            SunLight.Rotation = new Vector3(-Mathf.DegToRad(sunAngle), 0f, 0f);

            float visibility = GetSunVisibilityFactor(hour);
            float lightEnergy = Mathf.Lerp(GetTimeLightEnergy(hour), Night_LightEnergy, 1f - visibility);
            CurrentLightColor = GetTimeLightColor(hour).Lerp(Night_LightColor, 1f - visibility);
            CurrentCloudColor = GetCloudColor(hour).Lerp(Night_LightColor, 1f - visibility);
            CurrentFogColor = GetFogColor(hour).Lerp(Night_LightColor, 1f - visibility);
            SunLight.LightEnergy = lightEnergy;
            SunLight.LightColor = CurrentLightColor;

            // Shadow toggle during dawn/dusk
            if (EnableShadowControl && SunLight != null)
            {
                float vis = GetSunVisibilityFactor(hour);      // 0 = night, 1 = day
                SunLight.ShadowOpacity = vis;
            }
        }

        // Set sky colors based on time
        if (skyMaterial != null)
        {
            skyMaterial.SkyCoverModulate = CurrentCloudColor;
            Environment.FogLightColor = CurrentFogColor;
            skyMaterial.SkyTopColor = GetTimeSkyTopColor(hour);
            Color GroundColor = GetTimeSkyGroundColor(hour);
            skyMaterial.SkyHorizonColor = GroundColor;
            skyMaterial.GroundHorizonColor = GroundColor;
        }
    }


    /// 0 = night 1 = full daylight.  
    private float GetSunVisibilityFactor(float hour)
    {
        // Night 2 Dawn fade
        float dawnFadeStart = (AM_TimeStart - AM_TransitionTimeToDawn + 24f) % 24f;
        if (HourInRange(hour, dawnFadeStart, AM_TimeStart))
        {
            float t = ((hour - dawnFadeStart + 24f) % 24f) / AM_TransitionTimeToDawn;
            return Mathf.Clamp(t, 0f, 1f);              // 0 → 1
        }

        // Full day   (after dawn fade - before dusk fade)
        float duskFadeStart = EV_TimeEnd - EV_TransitionTimeToNight;   // 20 h
        if (HourInRange(hour, AM_TimeStart, duskFadeStart))
            return 1f;

        // Day / Night fade
        if (HourInRange(hour, duskFadeStart, EV_TimeEnd))
        {
            float t = (hour - duskFadeStart) / EV_TransitionTimeToNight;
            return 1f - Mathf.Clamp(t, 0f, 1f);         // 1 → 0
        }

        // True night
        return 0f;
    }

    /// ─────────────────────────────────────────────────────────────
    //  LIGHT  COLOR
    // ─────────────────────────────────────────────────────────────
    private Color GetTimeLightColor(float hour)
    {
        float nightFadeStart = (AM_TimeStart - AM_TransitionTimeToDawn + 24f) % 24f;

        if (HourInRange(hour, Night_TimeStart, nightFadeStart))
            return Night_LightColor;

        if (HourInRange(hour, nightFadeStart, AM_TimeStart))
        {
            float t = ((hour - nightFadeStart + 24f) % 24f) / AM_TransitionTimeToDawn;
            return Night_LightColor.Lerp(AM_DaytimeLightColor, Mathf.Clamp(t, 0f, 1f));
        }

        if (hour < AM_TimeEnd)
            return BlendToNext(hour, AM_TimeStart, AM_TimeEnd, AM_TransitionTimeToMorning,
                               AM_DaytimeLightColor, NOON_DaytimeLightColor,
                               (a, b, t) => a.Lerp(b, t));

        if (hour < NOON_TimeEnd)
            return BlendToNext(hour, NOON_TimeStart, NOON_TimeEnd, AM_TransitionTimeToAfternoon,
                               NOON_DaytimeLightColor, PM_DaytimeLightColor,
                               (a, b, t) => a.Lerp(b, t));

        if (hour < PM_TimeEnd)
            return BlendToNext(hour, PM_TimeStart, PM_TimeEnd, AM_TransitionTimeToDusk,
                               PM_DaytimeLightColor, EV_DaytimeLightColor,
                               (a, b, t) => a.Lerp(b, t));

        return BlendToNext(hour, EV_TimeStart, EV_TimeEnd, EV_TransitionTimeToNight,
                           EV_DaytimeLightColor, Night_LightColor,
                           (a, b, t) => a.Lerp(b, t));
    }

    /// ─────────────────────────────────────────────────────────────
    //  CLOUDS
    // ─────────────────────────────────────────────────────────────
    private Color GetCloudColor(float hour)
    {
        float nightFadeStart = (AM_TimeStart - AM_TransitionTimeToDawn + 24f) % 24f;

        if (HourInRange(hour, Night_TimeStart, nightFadeStart))
            return Night_CloudColor;

        if (HourInRange(hour, nightFadeStart, AM_TimeStart))
        {
            float t = ((hour - nightFadeStart + 24f) % 24f) / AM_TransitionTimeToDawn;
            return Night_CloudColor.Lerp(AM_CloudColor, Mathf.Clamp(t, 0f, 1f));
        }

        if (hour < AM_TimeEnd)
            return BlendToNext(hour, AM_TimeStart, AM_TimeEnd, AM_TransitionTimeToMorning,
                               AM_CloudColor, NOON_CloudColor,
                               (a, b, t) => a.Lerp(b, t));

        if (hour < NOON_TimeEnd)
            return BlendToNext(hour, NOON_TimeStart, NOON_TimeEnd, AM_TransitionTimeToAfternoon,
                               NOON_CloudColor, PM_CloudColor,
                               (a, b, t) => a.Lerp(b, t));

        if (hour < PM_TimeEnd)
            return BlendToNext(hour, PM_TimeStart, PM_TimeEnd, AM_TransitionTimeToDusk,
                               PM_CloudColor, EV_CloudColor,
                               (a, b, t) => a.Lerp(b, t));

        return BlendToNext(hour, EV_TimeStart, EV_TimeEnd, EV_TransitionTimeToNight,
                           EV_CloudColor, Night_CloudColor,
                           (a, b, t) => a.Lerp(b, t));
    }

    /// ─────────────────────────────────────────────────────────────
    //  FOG
    // ─────────────────────────────────────────────────────────────
    private Color GetFogColor(float hour)
    {
        float nightFadeStart = (AM_TimeStart - AM_TransitionTimeToDawn + 24f) % 24f;

        if (HourInRange(hour, Night_TimeStart, nightFadeStart))
            return Night_FogColor;

        if (HourInRange(hour, nightFadeStart, AM_TimeStart))
        {
            float t = ((hour - nightFadeStart + 24f) % 24f) / AM_TransitionTimeToDawn;
            return Night_FogColor.Lerp(AM_FogColor, Mathf.Clamp(t, 0f, 1f));
        }

        if (hour < AM_TimeEnd)
            return BlendToNext(hour, AM_TimeStart, AM_TimeEnd, AM_TransitionTimeToMorning,
                               AM_FogColor, NOON_FogColor,
                               (a, b, t) => a.Lerp(b, t));

        if (hour < NOON_TimeEnd)
            return BlendToNext(hour, NOON_TimeStart, NOON_TimeEnd, AM_TransitionTimeToAfternoon,
                               NOON_FogColor, PM_FogColor,
                               (a, b, t) => a.Lerp(b, t));

        if (hour < PM_TimeEnd)
            return BlendToNext(hour, PM_TimeStart, PM_TimeEnd, AM_TransitionTimeToDusk,
                               PM_FogColor, EV_FogColor,
                               (a, b, t) => a.Lerp(b, t));

        return BlendToNext(hour, EV_TimeStart, EV_TimeEnd, EV_TransitionTimeToNight,
                           EV_FogColor, Night_FogColor,
                           (a, b, t) => a.Lerp(b, t));
    }

    // ─────────────────────────────────────────────────────────────
    //  LIGHT  
    // ─────────────────────────────────────────────────────────────
    private float GetTimeLightEnergy(float hour)
    {
        float nightFadeStart = (AM_TimeStart - AM_TransitionTimeToDawn + 24f) % 24f;

        //  night
        if (HourInRange(hour, Night_TimeStart, nightFadeStart))
            return Night_LightEnergy;

        if (HourInRange(hour, nightFadeStart, AM_TimeStart))
        {
            float t = ((hour - nightFadeStart + 24f) % 24f) / AM_TransitionTimeToDawn;
            return Mathf.Lerp(Night_LightEnergy, AM_DaytimeLightEnergy, Mathf.Clamp(t, 0f, 1f));
        }

        // Dawn
        if (hour < AM_TimeEnd)
            return BlendToNext(hour, AM_TimeStart, AM_TimeEnd, AM_TransitionTimeToMorning,
                               AM_DaytimeLightEnergy, NOON_DaytimeLightEnergy,
                               Mathf.Lerp);

        // Morning
        if (hour < NOON_TimeEnd)
            return BlendToNext(hour, NOON_TimeStart, NOON_TimeEnd, AM_TransitionTimeToAfternoon,
                               NOON_DaytimeLightEnergy, PM_DaytimeLightEnergy,
                               Mathf.Lerp);

        // Afternoon
        if (hour < PM_TimeEnd)
            return BlendToNext(hour, PM_TimeStart, PM_TimeEnd, AM_TransitionTimeToDusk,
                               PM_DaytimeLightEnergy, EV_DaytimeLightEnergy,
                               Mathf.Lerp);

        // Dusk 2 Night
        return BlendToNext(hour, EV_TimeStart, EV_TimeEnd, EV_TransitionTimeToNight,
                           EV_DaytimeLightEnergy, Night_LightEnergy,
                           Mathf.Lerp);
    }

    // ─────────────────────────────────────────────────────────────
    //  SKY  TOP  COLOR
    // ─────────────────────────────────────────────────────────────
    private Color GetTimeSkyTopColor(float hour)
    {
        float nightFadeStart = (AM_TimeStart - AM_TransitionTimeToDawn + 24f) % 24f;

        if (HourInRange(hour, Night_TimeStart, nightFadeStart))
            return Night_SunTop;

        if (HourInRange(hour, nightFadeStart, AM_TimeStart))
        {
            float t = ((hour - nightFadeStart + 24f) % 24f) / AM_TransitionTimeToDawn;
            return Night_SunTop.Lerp(AM_DaytimeSunTop, Mathf.Clamp(t, 0f, 1f));
        }

        if (hour < AM_TimeEnd)
            return BlendToNext(hour, AM_TimeStart, AM_TimeEnd, AM_TransitionTimeToMorning,
                               AM_DaytimeSunTop, NOON_DaytimeSunTop,
                               (a, b, t) => a.Lerp(b, t));

        if (hour < NOON_TimeEnd)
            return BlendToNext(hour, NOON_TimeStart, NOON_TimeEnd, AM_TransitionTimeToAfternoon,
                               NOON_DaytimeSunTop, PM_DaytimeSunTop,
                               (a, b, t) => a.Lerp(b, t));

        if (hour < PM_TimeEnd)
            return BlendToNext(hour, PM_TimeStart, PM_TimeEnd, AM_TransitionTimeToDusk,
                               PM_DaytimeSunTop, EV_DaytimeSunTop,
                               (a, b, t) => a.Lerp(b, t));

        return BlendToNext(hour, EV_TimeStart, EV_TimeEnd, EV_TransitionTimeToNight,
                           EV_DaytimeSunTop, Night_SunTop,
                           (a, b, t) => a.Lerp(b, t));
    }


    // ─────────────────────────────────────────────────────────────
    //  SKY  GROUND  COLOR
    // ─────────────────────────────────────────────────────────────
    private Color GetTimeSkyGroundColor(float hour)
    {
        float nightFadeStart = (AM_TimeStart - AM_TransitionTimeToDawn + 24f) % 24f;

        if (HourInRange(hour, Night_TimeStart, nightFadeStart))
            return Night_Ground;

        if (HourInRange(hour, nightFadeStart, AM_TimeStart))
        {
            float t = ((hour - nightFadeStart + 24f) % 24f) / AM_TransitionTimeToDawn;
            return Night_Ground.Lerp(AM_DaytimeSunGround, Mathf.Clamp(t, 0f, 1f));
        }

        if (hour < AM_TimeEnd)
            return BlendToNext(hour, AM_TimeStart, AM_TimeEnd, AM_TransitionTimeToMorning,
                               AM_DaytimeSunGround, NOON_DaytimeSunGround,
                               (a, b, t) => a.Lerp(b, t));

        if (hour < NOON_TimeEnd)
            return BlendToNext(hour, NOON_TimeStart, NOON_TimeEnd, AM_TransitionTimeToAfternoon,
                               NOON_DaytimeSunGround, PM_DaytimeSunGround,
                               (a, b, t) => a.Lerp(b, t));

        if (hour < PM_TimeEnd)
            return BlendToNext(hour, PM_TimeStart, PM_TimeEnd, AM_TransitionTimeToDusk,
                               PM_DaytimeSunGround, EV_DaytimeSunGround,
                               (a, b, t) => a.Lerp(b, t));

        return BlendToNext(hour, EV_TimeStart, EV_TimeEnd, EV_TransitionTimeToNight,
                           EV_DaytimeSunGround, Night_Ground,
                           (a, b, t) => a.Lerp(b, t));
    }

    /// Allows external setting of in-game hour (0 to 24).
    public void SetHour(float hour)
    {
        currentTime = Mathf.Clamp(hour, 0f, 24f);
        UpdateLighting();
    }

    /// Returns true whenhr is in [start, end) on a 24‑hour clock.
    private static bool HourInRange(float hour, float start, float end)
    {
        return start < end
            ? hour >= start && hour < end
            : hour >= start || hour < end;
    }

    /// Returns a value smoothly blended
    private static T BlendToNext<T>(
        float hour,
        float segStart, float segEnd, float transitionToNext,
        T thisValue, T nextValue,
        Func<T, T, float, T> lerpFunc)
    {
        float blendStart = segEnd - transitionToNext;   // when the fade begins

        if (transitionToNext <= 0f || hour < blendStart || hour >= segEnd)
            return hour < segEnd ? thisValue : nextValue;

        // how deep are we into the fade?  
        float t = (hour - blendStart) / transitionToNext;
        return lerpFunc(thisValue, nextValue, Mathf.Clamp(t, 0f, 1f));
    }
}
