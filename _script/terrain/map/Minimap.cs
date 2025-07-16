using Bouncerock.Terrain;
using Godot;
using System;
using System.Collections.Generic;
using Bouncerock;
public partial class Minimap : Control
{
    [Export] public Label Angle;
    [Export] public TextureRect MinimapTexture;

    public float CurrentAngle = 0;

    public enum LocationToPoint { Forward, Zero }

    Vector2 precedentChunk = Vector2.Zero;

    public bool Initialized = false;

    public override void _Process(double delta)
    {
        Update();
    }

    public void Update()
    {
        UpdateMap();
        CallDeferred("updateMap");
    }

    void updateMap()
    {

    }


    public void UpdateMap()
    {
        if (TerrainManager.Instance == null)
        {
            return;
        }

        Vector2 currentChunk = TerrainManager.Instance.CameraInChunk();
        if (currentChunk != precedentChunk || !Initialized)
        {
            float[,] currentMap = TerrainManager.Instance.GetHeightmapForChunk(currentChunk); // Values between -200 and 200

            if (currentMap == null) return;

            // Convert heightmap to image
            Image minimapImage = GenerateMinimapImage(currentMap);

            // Resize the image to 500x500
            minimapImage.Resize(200, 200, Image.Interpolation.Lanczos);

            // Create a texture from the image
            ImageTexture minimapTexture = ImageTexture.CreateFromImage(minimapImage);

            // Display it in MinimapTexture
            MinimapTexture.Texture = minimapTexture;
            precedentChunk = currentChunk;
        }

    }

    

    private Image GenerateMinimapImage(float[,] heightmap)
    {
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);

        Image image = Image.CreateEmpty(width, height, false, Image.Format.Rgb8);

        float minHeight = -20;
        float maxHeight = 20;

        Vector2 PlayerLoc = TerrainManager.Instance.WorldspaceToChunkMapLocation(new Vector2(GameManager.Instance.GetMainCharacterPosition().X, GameManager.Instance.GetMainCharacterPosition().Z));
        Color orange = new Color(1, 0.5f, 0); // RGB for orange
        int x = 0;
        int y = 0;
       // GD.Print(x + "/" + y);
        for (x = 0; x < width; x++)
        {
            for (y = 0; y < height; y++)
            {
                
                float heightValue = heightmap[x, y];
                // Normalize the height value to range [0, 255]
                byte grayscale = (byte)((heightValue - minHeight) / (maxHeight - minHeight) * 255);

                // Set pixel color (grayscale)
                Color color = Colors.DarkBlue;
                if (heightValue > 0)
                {
                    color = new Color(grayscale / 255f, grayscale / 255f, grayscale / 255f);
                }
                image.SetPixel(width - 1 - x, y, color);
            }
        }
       x = width - 1 - Mathf.Clamp((int)PlayerLoc.X, 0, width - 1);
        y = Mathf.Clamp((int)PlayerLoc.Y, 0, height - 1);
        image.SetPixel(x, y, Colors.RebeccaPurple);
        List<List<WorldItemData>> worldItems = TerrainManager.Instance.GetItemsForChunk(precedentChunk);
        

        if (worldItems != null && worldItems.Count > 0)
        {
            // Overlay world items as orange dots
            
            foreach (List<WorldItemData> itemList in worldItems)
            {
                foreach (WorldItemData item in itemList)
                {
                    Vector2 itemPos = item.GridLocation;
                    if (item.Elevation>0)
                    {
                    y = Mathf.Clamp((int)itemPos.Y, 0, height - 1);
                   x = width - 1 - Mathf.Clamp((int)itemPos.X, 0, width - 1);

                    image.SetPixel(x, y, orange);
                    }
                   
                }
            }

            
        }

        //Initialized = true;
        return image;
    }

}
