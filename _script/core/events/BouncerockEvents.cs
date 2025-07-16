using Godot;

namespace Bouncerock.Events
{
	public struct EvtCameraChanged
	{
		public Camera3D NewCamera;
		public Camera3D OldCamera;
		 public EvtCameraChanged(Camera3D newCamera, Camera3D oldCamera)
            {
                NewCamera = newCamera;
				OldCamera = oldCamera;
            }
	}

	public struct EvtCharacterChanged
	{
		public MainCharacter NewCharacter;
		public MainCharacter OldCharacter;
		 public EvtCharacterChanged(MainCharacter newCharacter, MainCharacter oldCharacter)
            {
                NewCharacter = newCharacter;
				OldCharacter = oldCharacter;
            }
	}

	
}