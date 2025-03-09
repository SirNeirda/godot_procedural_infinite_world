using Godot;
using System;
using Bouncerock;
using Bouncerock.Terrain;
using Bouncerock.UI;

public partial class MainCharacter : CharacterBody3D
{
	[Export]
	public string CharacterName = "Bouncerock";

	[ExportGroup("Player Objects")]
	[Export]
	public Camera3D PlayerCamera;

	public WorldSpaceUI PopupInfo;

	[Export]
	public float Action = 100;


	[Export]
	public Node3D CameraPivot;

	public PackedScene Cube;

	[Export]
	public AnimationTree Animator;

	[ExportGroup("Player Attributes")]

	//Vector3 Direction = Vector3.Zero;

	Vector3 CameraRotationAxis = Vector3.Zero;
	

	float mouse_speed = 0.05f;

	[Export]
	public float WalkingSpeed = 4;
	[Export]
	public float WalkToRunSpeedIncrease = 1;
	[Export]
	public float RunningSpeed = 20;
	
	[Export]
	public float JumpVelocity = 5;
	/*[Export]
	float MaxSpeed = 4;*/
	float speed = 1;

	private float timeOffGround = 0f;

	float velocity = 0;

	public enum CharacterActions {Idle, Jumping, Running, Attacking, Walking, Falling, Gliding, Flying}

	public enum CharacterPowers {Crates, RotoPunch, Jetpack}

	public CharacterPowers CurrentPower = CharacterPowers.RotoPunch;

	public class CharacterInput
	{
		bool run = false;
		bool jump = false;

		bool attack = false;
		bool fly = false;
		public Vector3 Direction = Vector3.Zero;

		public Vector3 CameraDirection= Vector3.Zero;

		public void SetRun()
		{
			run = true;
			jump = false;
			attack = false;
			fly=false;
		}
		public void SetJump()
		{
			run = false;
			jump = true;
			attack = false;
			fly=false;
		}
		public void SetFly()
		{
			run = false;
			jump = false;
			attack = false;
			fly=true;
		}
		public void SetAttack()
		{
			run = false;
			jump = false;
			fly=false;
			attack = true;
		}
		public void EndAttack()
		{
			attack = false;
			
		}
		public void Reset()
		{
			run = false;
			jump = false;
			attack = false;
			fly=false;
		}

		public bool IsRunning(){return run;}
		public bool IsJumping(){return jump;}
		public bool IsAttacking(){return attack;}

		public bool IsFlying(){return fly;}
	} 

	public CharacterInput CurrentInput = new CharacterInput();

	public CharacterActions CurrentAction;
	

	float cam_rot_x=0;
	float cam_rot_y=0;

	[Export] public TouchInputManager touchInputManager;



	

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		
		Initialization();
	}

	protected virtual void Initialization()
		{
			
			Input.MouseMode = Input.MouseModeEnum.Captured;
			FloorMaxAngle = Mathf.DegToRad(50);
			GameManager.Instance.SetMainCamera(PlayerCamera);
			GameManager.Instance.SetMainCharacter(this);

			Cube = GD.Load<PackedScene>("res://_scenes/decor/crate.tscn");
		}

	public override void _PhysicsProcess(double delta)
		{
			
			/*float angleBoost = 1;
			if (IsOnFloor()) {angleBoost = Mathf.InverseLerp(0,50, Mathf.RadToDeg(GetFloorAngle()) * 2); }
			
			Velocity = new Vector3(Direction.X * (speed+angleBoost), Direction.Y * gravity, Direction.Z * (speed+angleBoost) );
			//Velocity -= gravityResistance;
			Velocity = Velocity.Rotated(Vector3.Up, Mathf.DegToRad(RotationDegrees.Y));
			//Velocity = Velocity* (float)delta * 60;
			Animator.Set("parameters/IdleWalkRun/blend_position", Direction.Z);

			MoveAndSlide();*/
		} 


	public override void _Process(double delta)
		{
			float deltaFloat = (float)delta;
			//GD.Print(RaycastDown.IsColliding());
			UpdateAction(deltaFloat);
			UpdateMovement(deltaFloat);
			UpdateCamera(deltaFloat);
			UpdateHelpers(deltaFloat);
			//UpdateInput(deltaFloat);
			UpdateAnimations();
		} 

	protected void UpdateAction(float deltaFloat)
	{
		CharacterActions previousAction = CurrentAction;
		if (CurrentInput.IsRunning() && CurrentInput.Direction != Vector3.Zero && Action >0)
		{
			if (IsOnFloor() || timeOffGround <0.5f)
			{
				CurrentAction = CharacterActions.Running; 
				timeOffGround =0;
				Action = Mathf.Clamp(Action - (deltaFloat * 10), 0, 100);
				return;
			}
			if (!IsOnFloor())
			{
				timeOffGround +=deltaFloat;
			}
		
		}
		if (CurrentInput.IsJumping() && !CurrentInput.IsFlying()){CurrentAction = CharacterActions.Jumping; return;}
		if (!IsOnFloor())
		{
			if (CurrentInput.IsFlying() && Action >0) {CurrentAction = CharacterActions.Flying; return;}
			CurrentAction = CharacterActions.Falling; return;
		}
		if (CurrentInput.IsAttacking() && Action >20)
		{
			Action = Mathf.Clamp(Action - 20, 0, 100);
			LaunchAttack();
			if (previousAction == CharacterActions.Attacking)
			{
				CurrentAction = CharacterActions.Attacking;
			}
			return;}
		if (CurrentInput.Direction != Vector3.Zero) 
		{
			Action = Mathf.Clamp(Action + deltaFloat,0,100);
			CurrentAction = CharacterActions.Walking; 
			return;
		}
		Action = Mathf.Clamp(Action + deltaFloat*3,0,100);
		CurrentAction = CharacterActions.Idle;
	}

	void LaunchAttack()
	{
		if (CurrentPower == CharacterPowers.Crates)
		{
			Animator.Set("parameters/conditions/tossing", true);
			RigidBody3D newCube = Cube.Instantiate() as RigidBody3D;
			GetTree().Root.AddChild(newCube);
			Vector3 forwardDirection = GlobalTransform.Basis.Z;

			newCube.Position = GlobalTransform.Origin + (forwardDirection*2)+Vector3.Up;

			Vector3 velocityDirection = (forwardDirection*2 + Vector3.Up).Normalized();
			newCube.LinearVelocity = velocityDirection * 10;
			CurrentInput.EndAttack();
		}
		if (CurrentPower == CharacterPowers.RotoPunch)
			{
				Animator.Set("parameters/conditions/tossing", true);
				
				CurrentInput.EndAttack();
			}

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
		Animator.Set("parameters/conditions/flying", false);
		if (CurrentAction == CharacterActions.Falling)
		{
			Animator.Set("parameters/conditions/falling", true);
			return;
		}
		if (CurrentAction == CharacterActions.Idle)
		{
			Animator.Set("parameters/conditions/idle", true);
			return;
		}
		if (CurrentAction == CharacterActions.Walking)
		{
			
			Animator.Set("parameters/conditions/running", true);
			return;
		}
		if (CurrentAction == CharacterActions.Running)
		{
			Animator.Set("parameters/conditions/sprinting", true);
			return;
		}
		if (CurrentAction == CharacterActions.Flying)
		{
			Animator.Set("parameters/conditions/flying", true);
			return;
		}
		
	}

	protected void ChangeAnimState()
	{

	}

	protected void UpdateMovement(float deltaFloat)
	{
		Vector3 velocity = Velocity;

		// Handle Jump.
		if (CurrentAction == CharacterActions.Jumping && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_right", "ui_left", "ui_down", "ui_up");
		CurrentInput.Direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (IsOnFloor())
		{
			if (Action >0)
			{
				speed = CurrentAction == CharacterActions.Running?RunningSpeed:WalkingSpeed;
			}
		}
		if (!IsOnFloor())
		{
			if (CurrentAction == CharacterActions.Flying)
			{
				Action = Mathf.Clamp(Action - (deltaFloat * 10), 0, 100);
				velocity.Y += 4 * deltaFloat;
				float t = Mathf.MoveToward((float)(speed - WalkingSpeed) / (RunningSpeed - WalkingSpeed), 0, 1);
				speed = Mathf.Lerp(WalkingSpeed, RunningSpeed, t);
			}
			else
			{
				Action = Mathf.Clamp(Action + (int)deltaFloat,0,100);
				velocity.Y -= gravity * deltaFloat;
				float t = Mathf.MoveToward((float)(speed - WalkingSpeed) / (RunningSpeed - WalkingSpeed), 0, 1);
				speed = Mathf.Lerp(WalkingSpeed, RunningSpeed, t);
			}
			
		}
		if (CurrentInput.Direction != Vector3.Zero)
		{
			velocity.X = CurrentInput.Direction.X * speed;
			velocity.Z = CurrentInput.Direction.Z * speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	protected void UpdateCamera(float deltaFloat)
		{
			#if GODOT_ANDROID
			//if (touchInputManager == null) {GD.Print("wtf"); return;}
			CameraRotationAxis.X = touchInputManager.CameraRotationAxis.X;
			CameraRotationAxis.Y= touchInputManager.CameraRotationAxis.Y;
			#endif
			float targetFov = 75;
			float fovLerpTime = 0.5f; // adjust this value to control the speed of the FOV change

			if (CurrentInput.Direction.Z != 0)
			{
				if (CurrentAction == CharacterActions.Running || CurrentAction == CharacterActions.Gliding)
				{
					targetFov = 100;
				}
				else
				{
					targetFov = 75;
				}
			}
			if (CameraRotationAxis!= Vector3.Zero)
			{
				cam_rot_x -= CameraRotationAxis.Y;
				cam_rot_y += Mathf.Clamp(CameraRotationAxis.X, -25, 60);
			} 
			PlayerCamera.Fov = Mathf.Lerp(PlayerCamera.Fov, targetFov, fovLerpTime * deltaFloat);

			Vector3 rotateCam = new Vector3(cam_rot_x, CameraPivot.RotationDegrees.Y, CameraPivot.RotationDegrees.Z );
			//this.RotationDegrees = this.RotationDegrees * (Vector3.up * 
			Vector3 rotateChar = new Vector3(RotationDegrees.X, cam_rot_y, RotationDegrees.Z );
			//CameraPivot.RotationDegrees = rotateChar;
			RotationDegrees = rotateChar;
			CameraPivot.RotationDegrees = rotateCam;
			//Rotation = (rotate.X);
			RotateY(Mathf.DegToRad(cam_rot_y));
			//RotateObjectLocal(Vector3.Right, Mathf.DegToRad(-pitch));
		}
	
	public void UpdateHelpers(float deltaFloat)
		{
			string text = CharacterName;
			if (PopupInfo == null)
			{
				PopupInfo = Debug.SetTextHelper(text, CameraPivot.Position, CameraPivot);
				PopupInfo.MaxViewDistance = 1000;
			}
			if (PopupInfo != null)
			{
				text = text+ "\n" + CurrentAction;
				//Mathf.RadToDeg(GetFloorAngle()) + " Pos: X: " + string.Format("{0:0. #}", Position.X) + " Y: " + string.Format("{0:0. #}", Position.Y) + " Z: " + string.Format("{0:0. #}", Position.Z) ;
				//GD.Print(TerrainGenerator.Instance.CameraInChunk());
				PopupInfo.SetText(text);
				PopupInfo.Position = CameraPivot.Position+ Vector3.Down*0.7f;
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
			CurrentInput.Reset();
			if (Input.IsActionPressed("run"))
			{
				CurrentInput.SetRun();
			}
			if (Input.IsActionPressed("jump"))
			{
				CurrentInput.SetJump();
			}
			if (Input.IsActionPressed("fly"))
			{
				CurrentInput.SetFly();
			}
			#if GODOT_WINDOWS 
			
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
					CurrentInput.SetAttack();
					
					//Toss a crate
					Animator.Set("parameters/conditions/tossing", true);
					RigidBody3D newCube = Cube.Instantiate() as RigidBody3D;
					GetTree().Root.AddChild(newCube);
					Vector3 forwardDirection = GlobalTransform.Basis.Z;

					newCube.Position = GlobalTransform.Origin + (forwardDirection*2)+Vector3.Up;

					Vector3 velocityDirection = (forwardDirection*2 + Vector3.Up).Normalized();
        			newCube.LinearVelocity = velocityDirection * 10;

				}
			}
			#endif 
			#if GODOT_ANDROID 
			if (Input.IsActionPressed("attack"))
			{
				CurrentInput.SetAttack();
					
					//Toss a crate
					Animator.Set("parameters/conditions/tossing", true);
					RigidBody3D newCube = Cube.Instantiate() as RigidBody3D;
					GetTree().Root.AddChild(newCube);
					Vector3 forwardDirection = GlobalTransform.Basis.Z;

					newCube.Position = GlobalTransform.Origin + (forwardDirection*2)+Vector3.Up;

					Vector3 velocityDirection = (forwardDirection*2 + Vector3.Up).Normalized();
        			newCube.LinearVelocity = velocityDirection * 10;
			}
			#endif 
			#if GODOT_WINDOWS 
			if (keyEvent is InputEventJoypadMotion joypadMotionEvent)
				{
					// Get the joystick axis values
					JoyAxis axis = joypadMotionEvent.Axis; // X-axis of the joystick
					if (axis == JoyAxis.RightX || axis == JoyAxis.RightY)//axis goes from -1 to 0
					{
						 // Get the joystick axis values
						 if (axis == JoyAxis.RightY)
						 {
							CameraRotationAxis.Y = joypadMotionEvent.AxisValue;
						 }
						  if (axis == JoyAxis.RightX)
						 {
							CameraRotationAxis.X = joypadMotionEvent.AxisValue;
						 }
					}
					//GD.Print(axis + joypadMotionEvent.AxisValue.ToString());
				}
				
			if (keyEvent is InputEventMouseMotion motion)
			{
				cam_rot_x = Mathf.Clamp((cam_rot_x +(-motion.Relative.Y * mouse_speed)), -25,60);
				cam_rot_y += -motion.Relative.X * mouse_speed;
			}
			#endif 
			if (Input.IsActionPressed("run"))
			{
				CurrentInput.SetRun();
				float height = TerrainManager.Instance.GetTerrainHeightAtGlobalCoordinate(new Vector2(GlobalPosition.X, GlobalPosition.Z));
				
				float degree = TerrainManager.Instance.GetTerrainInclinationAtGlobalCoordinate(new Vector2(GlobalPosition.X, GlobalPosition.Z));
				
				Vector3 location = new Vector3(GlobalPosition.X, height, GlobalPosition.Z);
				//GD.Print("Degree inclination: " + degree);
				
			}

		}
}
