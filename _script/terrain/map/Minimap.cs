////////////////////////////////////////////////////////////////////////////////////////////
/// This script is part of the project "Infinite Runner", a procedural generation project
/// By Adrien Pierret
/// 
/// Minimap: Minimap generator. NOT READY.
/// ///////////////////////////////////////////////////////////////////////////////////////

using Bouncerock.Terrain;
using Godot;
using System;
using System.Collections.Generic;
using Bouncerock;
public partial class Minimap : Control
{
    [Export] public Label Angle;
    [Export] public TextureRect MinimapTexture;

    [Export] public GridContainer ImagesContainer;

    List<Image> ImagesMosaic = new List<Image>();
    public float CurrentAngle = 0;

    public enum LocationToPoint { Forward, Zero }

    Vector2 precedentChunk = Vector2.Zero;

    public bool Initialized = false;

    public override void _Process(double delta)
    {
       // Update();
    }

    public void Update()
    {
        // UpdateMap();
        //CallDeferred("updateMap");
    }

    void updateMap()
    {

    }



    public void UpdateMap()
    {
        if (TerrainManager.Instance == null)
        { return; }

        Vector2 currentChunkLoc = TerrainManager.Instance.CameraInChunk();

        if (currentChunkLoc == precedentChunk && Initialized)
        {
            return;
        }


        foreach (Node child in ImagesContainer.GetChildren())
        {
            child.QueueFree();
        }

        ImagesMosaic.Clear();

        for (int y = -1; y <= 1; y++)        // top → bottom
        {
            for (int x = -1; x <= 1; x++)    // left → right
            {
                Vector2 chunkLoc = currentChunkLoc + new Vector2(x, y);
                TerrainChunk chunk = TerrainManager.Instance.GetChunk(chunkLoc);

                if (chunk == null)
                {
                    ImagesMosaic.Add(null);
                    continue;
                }

                Image chunkImage = CreateHeightmapImage(chunkLoc,
                    chunk,
                    chunk.GetHeightmap()
                );

                ImagesMosaic.Add(chunkImage);
            }
        }

        precedentChunk = currentChunkLoc;
        Initialized = true;


        foreach (Image img in ImagesMosaic)
        {
            ImageTexture texture = ConvertToTexture(img);
            TextureRect rect = new TextureRect();
            rect.Texture = texture;
            ImagesContainer.CallDeferred("add_child", rect);
        }
    }

    private Image CreateHeightmapImage(Vector2 chunkLoc, TerrainChunk chunk, float[,] heightmap, int trimPerSide = 0)
    {
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);

        trimPerSide = Mathf.Clamp(trimPerSide, 0, Mathf.Min(width, height) / 2);
        int totalTrim = trimPerSide * 2;

        int trimmedWidth = width - totalTrim;
        int trimmedHeight = height - totalTrim;

        Color playerColor = Colors.RebeccaPurple;
        Color itemColor = new Color(1, 0.5f, 0); // orange

        Image img = Image.CreateEmpty(trimmedWidth, trimmedHeight, false, Image.Format.Rgbaf);

        float maxHeight = 200;
        float minHeight = 0;



        for (int x = 0; x < trimmedWidth; x++)
        {
            for (int y = 0; y < trimmedHeight; y++)
            {
                // Read original heightmap (not flipped)
                float h = heightmap[x + trimPerSide, y + trimPerSide];

                //  flipped on Y axis
                int flippedY = trimmedHeight - 1 - y;
                img.SetPixel(x, flippedY, GetColorByHeight(h));
            }
        }

        //  Player marker
        Vector2 playerLoc = TerrainManager.Instance.WorldspaceToChunkMapLocation(
          new Vector2(GameManager.Instance.GetMainCharacterPosition().X, GameManager.Instance.GetMainCharacterPosition().Z)
      );
        if (playerLoc == chunkLoc)
        {
            int px = width - 1 - Mathf.Clamp((int)playerLoc.X, 0, width - 1);
            int py = Mathf.Clamp((int)playerLoc.Y, 0, height - 1);
            DrawDot(img, px, py, 5, playerColor);//img.SetPixel(px, py, playerColor);
        }


        //World items overlay
        List<WorldItem> worldItems = TerrainManager.Instance.GetItemsForChunk(chunkLoc);
        //GD.Print("items " + worldItems.Count);
        if (worldItems != null)
        {
                foreach (WorldItem item in worldItems)
                {
                    if (item.WorldPosition.Y > 0)
                    {
                        int ix = width - 1 - Mathf.Clamp((int)item.GridLocation.X, 0, width - 1);
                        int iy = Mathf.Clamp((int)item.GridLocation.Y, 0, height - 1);
                        GD.Print("items " + item.WorldPosition.Y);
                        DrawDot(img, ix, iy, 5, itemColor);
                        //img.SetPixel(ix, iy, itemColor);
                    }
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

    private void DrawDot(Image img, int cx, int cy, int radius, Color color)
    {
        int w = img.GetWidth();
        int h = img.GetHeight();

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y > radius * radius)
                    continue; // keep it circular

                int px = cx + x;
                int py = cy + y;

                if (px < 0 || px >= w || py < 0 || py >= h)
                    continue;

                img.SetPixel(px, py, color);
            }
        }
    }
    private Color GetColorByHeight(float height)
    {
        //  Water 
        if (height < 0f)
            return new Color(0f, 0f, 1f, 1f); // Blue

        //  Sand
        if (height <= 3f)
            return new Color(1f, 1f, 0f, 1f); // Yellow

        // Normalize elevation (0  200)
        float t = Mathf.Clamp(height / 100f, 0f, 1f);

        // Green White interpolation
        float r = Mathf.Lerp(0f, 1f, t);
        float g = 1f; // green channel stays max
        float b = Mathf.Lerp(0f, 1f, t);

        Color baseColor = new Color(r, g, b, 1f);

        // Contour lines
        const float contourStep = 10f;
        const float contourThickness = 0.2f;

        float mod = Mathf.Abs(height % contourStep);

        if (mod < contourThickness || mod > contourStep - contourThickness)
        {
            // Darken contour lines
            baseColor = new Color(
                baseColor.R * 0.4f,
                baseColor.G * 0.4f,
                baseColor.B * 0.4f,
                1f
            );
        }

        return baseColor;
    }


}
