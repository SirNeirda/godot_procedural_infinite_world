using Godot;
using System;

public partial class ButtonMobile : Button
{
    [Export] public string ActionName = ""; // Set this to the input action to triger

    public override void _Ready()
    {
        // Connect button signals
        ButtonDown += OnButtonPressed;
        ButtonUp += OnButtonReleased;
    }

    private void OnButtonPressed()
    {
        Input.ActionPress(ActionName); // Simulate pressing the action
    }

    private void OnButtonReleased()
    {
        Input.ActionRelease(ActionName); 
    }
}
