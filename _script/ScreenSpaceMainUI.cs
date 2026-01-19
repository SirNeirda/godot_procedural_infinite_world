using Godot;
using System;

public partial class ScreenSpaceMainUI : Control
{
	// Called when the node enters the scene tree for the first time.

	[Export]
	public Label Distance;
	[Export]
	public ProgressBar ActionBar;

	[Export]
	public ProgressBar MojoBar;

	[Export]
	public Label Score;

	[Export]
	public Label Multiplier;

	[Export]
	public Label Record;
	[Export]
	public Label TimeOfDay;

	public override void _Ready()
	{

	}

	void UpdateTime()

	{
		CallDeferred("updateTimeOfDay");
	}




	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		UpdateTime();
		UpdateDistance();
		UpdateActionBar();
		UpdateRecord();
	}

	void UpdateDistance()

	{
		CallDeferred("updateDistance");
	}

	void UpdateActionBar()

	{
		CallDeferred("updateActionBar");
	}

	void updateActionBar()
	{
		ActionBar.Value = GameManager.Instance.GetMainCharacterAction();
		MojoBar.Value = GameManager.Instance.GetMainCharacterMojo();
		Score.Text = GameManager.Instance.GetMainCharacterScore().ToString();
		float multiplier = GameManager.Instance.StartingPoint.DistanceTo(GameManager.Instance.GetMainCharacterPosition());
				multiplier = Mathf.Clamp(multiplier/100,1,100);
				multiplier = Mathf.FloorToInt(multiplier);
				Multiplier.Text = "x" + multiplier;
	}

	void UpdateRecord()

	{
		CallDeferred("updateRecord");
	}

	void updateRecord()
	{
		float curdist = GameManager.Instance.StartingPoint.DistanceTo(GameManager.Instance.GetMainCharacterPosition());
		if (GameManager.Instance.Record < curdist)
		{
			Record.Text = curdist.ToString();
			GameManager.Instance.Record = curdist;
		}
	}

	void updateDistance()
	{
		Distance.Text = Mathf.FloorToInt(GameManager.Instance.StartingPoint.DistanceTo(GameManager.Instance.GetMainCharacterPosition())).ToString();
	}

	void updateTimeOfDay()
	{
		TimeOfDay.Text = EnvironmentManager.Instance.GetTime();
	}
}
