////////////////////////////////////////////////////////////////////////////////////////////
/// This script is part of the project "Infinite Runner", a procedural generation project
/// By Adrien Pierret
/// 
/// LoadingScreen is a simple screen loader.
/// ///////////////////////////////////////////////////////////////////////////////////////

using Godot;
using System;

public partial class LoadingScreen : Control
{
    [Export] public Label LoadingText;

    [Export] public PanelContainer LoadingContainer;
    [Export] public GradientTexture2D BackgroundGradientLoader;
    public void SetLoadingText(string text)
    {
        LoadingText.Text = text;
    }


}
