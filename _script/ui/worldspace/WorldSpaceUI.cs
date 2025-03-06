using Godot;
using System;

public partial class WorldSpaceUI : Node3D
{
	Control child;

	

	[Export]
	public float MaxViewDistance = 30;


	[Export]
	public float MinViewDistance = 0;


	public override void _Ready()
	{
		child = GetNode<Control>("Control");
		if (GameManager.Instance.MainCamera == null) {return;}
		Vector2 screenPosition = GameManager.Instance.MainCamera.UnprojectPosition(GlobalPosition);
		child.GlobalPosition = screenPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (GameManager.Instance.MainCamera == null) {;return;}
		if (GlobalPosition.DistanceTo(GameManager.Instance.MainCamera.GlobalPosition) > MaxViewDistance ||
			GameManager.Instance.MainCamera.IsPositionBehind(GlobalPosition))
		{
			child.Visible = false;
			return;
		}
		if (!child.IsProcessing()) {child.Visible = true;}
		Vector2 screenPosition = GameManager.Instance.MainCamera.UnprojectPosition(GlobalPosition);
		
		child.Position = screenPosition;
	}

	public void SetText(string text)
	{
		GetNode<Label>("Control/Label").Text = text;
	}
}
