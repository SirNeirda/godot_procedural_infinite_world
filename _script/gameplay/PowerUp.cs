using Godot;
using System;
using Bouncerock.Terrain;

public partial class PowerUp : WorldItemModel
{
	// Called when the node enters the scene tree for the first time.

	[Export]
	public CollisionShape3D collider;

		[Export]
	public GpuParticles3D particles;

	[Export]
	public MeshInstance3D mesh;

	[Export]
	public OmniLight3D light;

	bool consumed = false;

    public override void _Ready()
    {
       // light.Visible = true;
    }

	public override void OnChangedLOD(int lod)
	{
		//GD.Print("LOD Changed" + lod);
		if (lod == 0)
		{
			light.Visible = true;
		}
	} 


	public  void _on_area_3d_body_entered(Node3D area)
	{
		if (area is MainCharacter && !consumed)
		{
			consumed = true;
			MainCharacter chara = area as MainCharacter;
			chara.AddAction(15);
			particles.Visible = true;
			particles.Emitting = true;
			//QueueFree();
			PlayConsumeAnimation();
		}
		
	}

	private void PlayConsumeAnimation()
	{

		Tween tween = CreateTween();//Tweens are basically like Unity coroutines but for animations only. So here we're just putting animations inside one function.

		tween.TweenProperty(mesh, "scale", Vector3.Zero, 0.2f);
		tween.TweenProperty(mesh, "position", mesh.Position + Vector3.Up * 0.5f, 0.2f);

		tween.TweenCallback(Callable.From(QueueFree));
	}

}
