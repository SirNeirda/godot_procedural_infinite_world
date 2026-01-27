////////////////////////////////////////////////////////////////////////////////////////////
/// This script is part of the project "Infinite Runner", a procedural generation project
/// By Adrien Pierret
/// 
/// CharacterMob: This manages the generation and movement of characters.
/// ///////////////////////////////////////////////////////////////////////////////////////
using Godot;
using System;
using Bouncerock;
using Bouncerock.Terrain;
using Bouncerock.UI;

public partial class CharacterMob : CharacterBody3D
{
	[Export]
	public string CharacterName = "Mob";

	[Export] public bool ShowName = false;

	[ExportGroup("Player Objects")]

	public WorldSpaceUI PopupInfo;


	[Export]
	public Node3D CameraPivot;


	[Export]
	public AnimationTree Animator;

	[ExportGroup("Player Attributes")]

	Vector3 Direction = Vector3.Zero;


	float mouse_speed = 0.05f;

	[Export]
	public float WalkingSpeed = 6;
	[Export]
	public float WalkToRunSpeedIncrease = 1;
	[Export]
	public float RunningSpeed = 20;

	[Export]
	public float PunchedVelocity = 10;
	/*[Export]
	float MaxSpeed = 4;*/
	float speed = 1;

	float velocity = 0;

	[Export] public float DirectionUpdateInterval = 2f;
	[Export] public float RotationSmoothness = 4f;

	private float _directionTimer = 5f;
	private Vector3 _cachedDirection;


	public enum MobStates { Aggressive, Punched, Passive }

	public MobStates CurrentState = MobStates.Passive;
	// Get the gravity from the project settings to be synced with RigidBody nodes.

	bool hasTouchedGround = false;
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{

		Initialization();
	}

	protected virtual void Initialization()
	{
		Velocity = Vector3.Zero;
		RandomNumberGenerator rng = new RandomNumberGenerator();
		int multiplier = rng.RandiRange(-5, 5);
		_cachedDirection = _cachedDirection*multiplier;
		//Input.MouseMode = Input.MouseModeEnum.Captured;
		FloorMaxAngle = Mathf.DegToRad(50);

	}

	public override void _PhysicsProcess(double delta)
	{
		float deltaFloat = (float)delta;
		
		UpdateMovement(deltaFloat);
	}


	public override void _Process(double delta)
	{
		float deltaFloat = (float)delta;
		//GD.Print(RaycastDown.IsColliding());
		UpdateVelocity(deltaFloat);
		//UpdateCamera(deltaFloat);
		UpdateHelpers(deltaFloat);
		//UpdateInput(deltaFloat);
		UpdateAnimations();
	}

	private void UpdateVelocity(float delta)
	{
		_directionTimer -= delta;
		if (_directionTimer <= 0f)
		{
			_directionTimer = DirectionUpdateInterval;
			_cachedDirection = GetDirection();
		}

		Vector3 velocity = Velocity;

		if (CurrentState == MobStates.Punched && IsOnFloor())
		{
			velocity.Y = PunchedVelocity;
		}

		Vector3 forward = GlobalTransform.Basis.Z;
		float speed = WalkingSpeed;

		if (IsOnFloor())
		{
			velocity.Y = 0;
			velocity.X = forward.X * speed;
			velocity.Z = forward.Z * speed;
		}
		else
		{
			
			velocity.Y -= Mathf.Max(gravity * delta, gravity);
		}
		/*if (velocity.X  >2|| velocity.Z  >2)
		{
			GD.Print(Name + ": velocity " + velocity);
		}*/
		
		Velocity = velocity;
	}



	/*public override void _Input(InputEvent keyEvent)
		{
			
			if (keyEvent is InputEventMouseButton _mouseButton)
			{
				switch (_mouseButton.ButtonIndex)
				{
					case MouseButton.Right:
					Input.MouseMode = _mouseButton.Pressed? Input.MouseModeEnum.Captured:Input.MouseModeEnum.Visible;
					break;
				}
				if (_mouseButton.ButtonIndex == MouseButton.Left && _mouseButton.Pressed)
				{
					RigidBody3D newCube = Cube.Instantiate() as RigidBody3D;
					GetTree().Root.AddChild(newCube);
					Vector3 forwardDirection = GlobalTransform.Basis.Z;

					newCube.Position = GlobalTransform.Origin + (forwardDirection*2)+Vector3.Up;

					Vector3 velocityDirection = (forwardDirection*2 + Vector3.Up).Normalized();
        			newCube.LinearVelocity = velocityDirection * 5;

					//newCube.Position = this.Position + Vector3.Back +Vector3.Up;
				//	newCube.Rotation = this.Rotation;
					//newCube.LinearVelocity = (Vector3.Back+Vector3.Up)*10;
				}
			}
			if (keyEvent is InputEventMouseMotion motion)
			{
				cam_rot_x = Mathf.Clamp((cam_rot_x +(-motion.Relative.Y * mouse_speed)), -25,60);
				cam_rot_y += -motion.Relative.X * mouse_speed;
			}
			if (Input.IsActionPressed("action"))
			{
				float height = TerrainManager.Instance.GetTerrainHeightAtGlobalCoordinate(new Vector2(GlobalPosition.X, GlobalPosition.Z));

				float degree = TerrainManager.Instance.GetTerrainInclinationAtGlobalCoordinate(new Vector2(GlobalPosition.X, GlobalPosition.Z));

				Vector3 location = new Vector3(GlobalPosition.X, height, GlobalPosition.Z);
				GD.Print("Degree inclination: " + degree);
				
				
			}

		}*/

	protected void UpdateAnimations()
	{

		Animator.Set("parameters/conditions/falling", false);
		Animator.Set("parameters/conditions/idle", false);
		Animator.Set("parameters/conditions/sprinting", false);
		Animator.Set("parameters/conditions/running", false);
		Animator.Set("parameters/conditions/tossing", false);
		
		if (!IsOnFloor())
		{
			Animator.Set("parameters/conditions/falling", true);
			return;
		}
		if (Velocity.Z == 0 && IsOnFloor())
		{
			Animator.Set("parameters/conditions/idle", true);
			return;
		}
		if (Velocity.Z != 0)
		{
			if (Input.IsActionPressed("run"))
			{
				Animator.Set("parameters/conditions/sprinting", true);
				return;
			}
			Animator.Set("parameters/conditions/running", true);
			return;
		}

	}

	protected void ChangeAnimState()
	{

	}

	protected void ChangeCharacterState(MobStates newState)
	{
		if (newState != CurrentState)
		{
			CurrentState = newState;
		}
	}


	public void DeathPunched()
	{
		ChangeCharacterState(MobStates.Punched);

	}


	public Vector3 GetDirection()
	{
		return GameManager.Instance.GetMainCharacterPosition();
	}

	private void UpdateMovement(float delta)
	{
		if (GetDirection().DistanceTo(Position) < 1.5f)
		{
			GameManager.Instance.OnLoseGame();
		}
		// --- Rotation ---
		if (_cachedDirection != Vector3.Zero)
		{
			Vector3 lookDir = (_cachedDirection - GlobalTransform.Origin).Normalized();

			// Prevent zero or invalid direction
			if (lookDir.LengthSquared() > 0.0001f)
			{
				Basis currentBasis = GlobalTransform.Basis.Orthonormalized(); // ensure normalization
				Basis targetBasis = Basis.LookingAt(-lookDir, Vector3.Up).Orthonormalized();

				Basis smoothBasis = currentBasis.Slerp(targetBasis, delta * RotationSmoothness).Orthonormalized();

				GlobalTransform = new Transform3D(
					smoothBasis,
					GlobalTransform.Origin
				);
			}
		}

		MoveAndSlide();
	}


	protected void UpdateCamera(float deltaFloat)
	{
	}

	public void UpdateHelpers(float deltaFloat)
	{
		if (!ShowName) {return;}
		string text = CharacterName;
		string dist = GetDirection().DistanceTo(Position).ToString("0.00");
		if (PopupInfo == null)
		{
			PopupInfo = Debug.SetTextHelper(text, CameraPivot.Position, CameraPivot);
			PopupInfo.MaxViewDistance = 30;
		}
		if (PopupInfo != null)
		{
			//text = text+ "\n" + Mathf.RadToDeg(GetFloorAngle()) + " Pos: X: " + string.Format("{0:0. #}", Position.X) + " Y: " + string.Format("{0:0. #}", Position.Y) + " Z: " + string.Format("{0:0. #}", Position.Z) ;
			//GD.Print(TerrainGenerator.Instance.CameraInChunk());
			PopupInfo.SetText(text);
			PopupInfo.Position = CameraPivot.Position + Vector3.Down * 0.7f;
			//GD.Print("Position " + PopupInfo.Position);
		}
		

		/*string text = "Adrien" + "\n" + TerrainManager.Instance.CameraInChunk();

		if (PopupInfo == null)
		{
			PopupInfo = Debug.SetTextHelper(text, CameraPivot.Position, CameraPivot);
			PopupInfo.MaxViewDistance = 1000;
		}
		if (PopupInfo != null)
		{
			text = "AdrienSetup" + "\n" + Mathf.RadToDeg(GetFloorAngle()) + " Pos: \nX: " + string.Format("{0:0. #}", Position.X) + "\nY: " + string.Format("{0:0. #}", Position.Y) + "\nZ: " + string.Format("{0:0. #}", Position.Z) ;
			//GD.Print(TerrainGenerator.Instance.CameraInChunk());
			PopupInfo.SetText(text);
			//PopupInfo.Position = CameraPivot.Position+ Vector3.Up *0.2f;
		}*/
	}

	public override void _Input(InputEvent keyEvent)
	{



	}
}
