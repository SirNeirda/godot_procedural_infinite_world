using Godot;
using System.Collections.Generic;

public partial class TouchInputManager : Node
{
    //quick and probably flawed mobile movement managers for touch screens
    #if GODOT_ANDROID
    private Dictionary<int, Vector2> activeTouches = new Dictionary<int, Vector2>(); // Track multiple touches
    private int movementTouchIndex = -1; 
    private int cameraTouchIndex = -1;   

    private Vector2 movementStartPos = Vector2.Zero;
    //private Vector2 cameraStartPos = Vector2.Zero;
    private bool isDraggingMovement = false;
    private bool isDraggingCamera = false;

    [Export] public float DragThreshold = 50f; // Min distance to register movement
     [Export] public float RunThreshold = 350f; // Min distance to register ruuuuuuun

    public Vector2 CameraRotationAxis { get; private set; } = Vector2.Zero;
    private Dictionary<string, bool> activeActions = new Dictionary<string, bool>();

    Vector2  lastCameraTouchPos = Vector2.Zero;
    private bool isFirstCameraTouch = true;

    public override void _Process(double delta)
    {
        // Maintain active actions
        foreach (var action in activeActions)
        {
            if (action.Value)
                Input.ActionPress(action.Key);
            else
                Input.ActionRelease(action.Key);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventScreenTouch touchEvent)
        {
            if (touchEvent.Pressed)
                OnTouchStart(touchEvent.Position, touchEvent.Index);
            else
                OnTouchEnd(touchEvent.Index);
        }
        else if (@event is InputEventScreenDrag dragEvent)
        {
            OnTouchDrag(dragEvent.Position, dragEvent.Index);
        }
    }

    private void OnTouchStart(Vector2 position, int index)
    {
        float screenWidth = GetViewport().GetVisibleRect().Size.X;

        if (position.X < screenWidth / 2) // Left side: Movement
        {
            if (movementTouchIndex == -1) // Only track one movement touch
            {
                movementTouchIndex = index;
                movementStartPos = position;
                isDraggingMovement = true;
            }
        }
        else // Right side: Camera Control
        {
            if (cameraTouchIndex == -1) // Only track one camera touch
            {

                cameraTouchIndex = index;
                lastCameraTouchPos = position;
                isDraggingCamera = true;
            }
        }

        activeTouches[index] = position;
    }

    private void OnTouchEnd(int index)
    {
        if (index == movementTouchIndex) // Only stop movement if this was the movement touch
        {
            StopMovement();
            Input.ActionRelease("run");
            movementTouchIndex = -1;
            isDraggingMovement = false;
        }

        if (index == cameraTouchIndex) // Only stop camera control if this was the camera touch
        {
            CameraRotationAxis = Vector2.Zero;
            //lastCameraTouchPos = Vector2.Zero;
            cameraTouchIndex = -1;
            isDraggingCamera = false;
        }

        activeTouches.Remove(index);
    }

    private void OnTouchDrag(Vector2 position, int index)
    {
       
        if (index == movementTouchIndex && isDraggingMovement)
        {
            Vector2 delta = position - movementStartPos;
            if (delta.Length() < DragThreshold) return; 
            string action = GetMoveDirection(delta);
            if (delta.Length() > RunThreshold) {Input.ActionPress("run");}
            else {Input.ActionRelease("run");}
            if (action != null)
                StartAction(action);
        }
        else if (index == cameraTouchIndex && isDraggingCamera) 
        {
            /*if (isFirstCameraTouch)
            {
                isFirstCameraTouch = false;
            }*/

            Vector2 delta = position - lastCameraTouchPos; 
            float cameraSensitivity = 0.05f; 
            GD.Print("axis" + CameraRotationAxis);
            CameraRotationAxis = new Vector2(-delta.X * cameraSensitivity, -delta.Y * cameraSensitivity);
            //lastCameraRotationAxis = CameraRotationAxis;
            lastCameraTouchPos = position; 
        }
            

    }

    private void OnTouchRelease(int index)
{
   /* if (index == cameraTouchIndex)
    {
        isFirstCameraTouch = true; 
    }*/
}

    private string GetMoveDirection(Vector2 delta)
    {
        if (Mathf.Abs(delta.X) > Mathf.Abs(delta.Y)) // Horizontal movement
        {
            return delta.X > 0 ? "ui_right" : "ui_left";
        }
        else // Vertical movement
        {
            return delta.Y > 0 ? "ui_down" : "ui_up";
        }
    }

    private void StopMovement()
    {
        StopAction("ui_up");
        StopAction("ui_down");
        StopAction("ui_left");
        StopAction("ui_right");
    }

    public void StartAction(string actionName)
    {
        activeActions[actionName] = true;
    }

    public void StopAction(string actionName)
    {
        activeActions[actionName] = false;
    }
    #endif
}
