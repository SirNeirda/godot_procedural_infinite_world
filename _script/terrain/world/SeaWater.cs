////////////////////////////////////////////////////////////////////////////////////////////
/// This script is part of the project "Infinite Runner", a procedural generation project
/// By Adrien Pierret
/// 
/// SeaWater: This script will turn the heightmap into a water shader friendly texture.
/// ///////////////////////////////////////////////////////////////////////////////////////

using Bouncerock;
using Bouncerock.Terrain;
using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class SeaWater : Node3D
{


    [Export] public float SeaLevelY = 0.0f;



    private Camera3D _camera;

    [Export] public MeshInstance3D WaterMesh;

    [Export] public ShaderMaterial DebugShader;


    [Export] public bool UseDebug;
    protected bool locked = false;

    private float updateShadertimer = 0f;
    private float updateShaderinterval = 10f; // seconds
    /*[Export] string ShaderAddress;

    [Export] string DebugShaderAddress;
    [Export] bool UseDebug = true;*/


    public override void _Ready()
    {
        _camera = GameManager.Instance?.MainCamera;


        if (_camera == null)
        {
            GD.PushError($"{Name}: GameManager.Instance.MainCamera is null!");
        }

    }

    public override void _Process(double delta)
    {
        updateShadertimer += (float)delta;
        if (updateShadertimer >= updateShaderinterval)
        {
            updateShadertimer = 0f;
            //UpdateShaderColorScheme();
        }
    }

    void UpdateShaderColorScheme()
    {
        ShaderMaterial shaderMat = WaterMesh.GetSurfaceOverrideMaterial(0) as ShaderMaterial;
        shaderMat.SetShaderParameter("rim_color", EnvironmentManager.Instance.CurrentLightColor);
    }
    async public Task SetTerrainElevation(string chunkaddress, float[,] heightmap)
    {
        if (WaterMesh == null)
        {
            GD.PrintErr($"{Name}: WaterMesh is missing!");
            return;
        }
        if (locked)
        {
            GD.Print("Locked."); return;
        }

        //  Load shader resource
        //string path = UseDebug ? DebugShaderAddress : ShaderAddress;

        // Shader shader = ResourceLoader.Load<Shader>(path);

        /*if (shader == null)
        {
            GD.PrintErr($"{Name}: Failed to load shader at: {path}");
            return;
        }*/
        Image img = CreateHeightmapImage(chunkaddress, heightmap);

        Texture2D tex = ConvertToTexture(img);
        ShaderMaterial uniqueMat = new ShaderMaterial();
        if (UseDebug)
        {
            uniqueMat = DebugShader.Duplicate() as ShaderMaterial;
        }
        else
        {
            ShaderMaterial shaderMat = WaterMesh.GetSurfaceOverrideMaterial(0) as ShaderMaterial;
            uniqueMat = shaderMat.Duplicate() as ShaderMaterial;
        }
        //shaderMat.Shader = TerrainDetailsManager.Instance.WaterShader;

        uniqueMat.SetShaderParameter("heightmap_tex", tex);



        //uniqueMat.SetShaderParameter("rim_color", EnvironmentManager.Instance.CurrentLightColor);

        WaterMesh.SetSurfaceOverrideMaterial(0, uniqueMat);



        // Create heightmap texture


        //CallDeferred(nameof(ShowDebugTexture), tex);
    }

    private void ShowDebugTexture(Texture2D tex)
    {
        var textureRect = new TextureRect();
        textureRect.Texture = tex;    // Scale to fit
        textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspect;

        // Optional: set size and position
        textureRect.CustomMinimumSize = new Vector2(256, 256);
        textureRect.Modulate = Colors.White;

        // Add to UI
        var canvasLayer = new CanvasLayer();
        canvasLayer.AddChild(textureRect);
        GetTree().Root.AddChild(canvasLayer);
    }

    private Image CreateDebugImage(float[,] heightmap, int cellSize = 8)
    {
        RandomNumberGenerator rng = new RandomNumberGenerator();

        rng.Randomize();

        // Random cell size (useful for testing texture uniqueness)
        cellSize = rng.RandiRange(1, cellSize);
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);
        // Use RGB8 so it's visible and easy to debug
        Image img = Image.CreateEmpty(width, height, false, Image.Format.Rgb8);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool xCheck = (x / cellSize) % 2 == 0;
                bool yCheck = (y / cellSize) % 2 == 0;

                Color color = (xCheck ^ yCheck)   // XOR → alternating
                    ? Colors.White
                    : Colors.Black;

                img.SetPixel(x, y, color);
            }
        }

        return img;
    }

    private Image CreateCheckerImage(float[,] heightmap, int blockSize = 16)
    {
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);
        Image img = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Determine checker square
                bool isBlack = ((x / blockSize) + (y / blockSize)) % 2 == 0;

                Color color = isBlack ? Colors.Black : Colors.White;
                img.SetPixel(x, y, color);
            }
        }

        return img;
    }

    private Image CreateHeightmapImage(string chnkaddress, float[,] heightmap, int trimPerSide = 0)
    {
        int width = heightmap.GetLength(0);
    int height = heightmap.GetLength(1);

    trimPerSide = Mathf.Clamp(trimPerSide, 0, Mathf.Min(width, height) / 2);
    int totalTrim = trimPerSide * 2;

    int trimmedWidth = width - totalTrim;
    int trimmedHeight = height - totalTrim;

    Image img = Image.CreateEmpty(trimmedWidth, trimmedHeight, false, Image.Format.Rf);

    float maxHeight = TerrainManager.Instance.CurrentMapSettings.HighestPoint;
    float minHeight = -20;
    float range = 0 - minHeight;

    float noiseStrength = 0.02f;

    for (int x = 0; x < trimmedWidth; x++)
    {
        for (int y = 0; y < trimmedHeight; y++)
        {
            // Read original heightmap (not flipped)
            float h = heightmap[x + trimPerSide, y + trimPerSide];

            float normalized = (h - minHeight) / range;
            normalized = Mathf.Clamp(normalized, 0.0f, 1.0f);

            float noise = (GD.Randf() * 2f - 1f) * noiseStrength;
            normalized = Mathf.Clamp(normalized + noise, 0f, 1f);

            //  flipped on Y axis
            int flippedY = trimmedHeight - 1 - y;

            img.SetPixel(x, flippedY, new Color(normalized, 0, 0));
        }
    }

        // Save if needed

        /* string docs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
         string folder = System.IO.Path.Combine(docs, "Somewhere");

         if (!System.IO.Directory.Exists(folder))
             System.IO.Directory.CreateDirectory(folder);

         string filePath = System.IO.Path.Combine(folder, $"heightmap_debug{chnkaddress}.png");

         img.SavePng(filePath);
         GD.Print($"Saved heightmap to {filePath}");*/


        return img;
    }
    private ImageTexture ConvertToTexture(Image img)
    {
        ImageTexture tex = new ImageTexture();
        // tex.
        tex.SetImage(img);
        return tex;
    }
}
