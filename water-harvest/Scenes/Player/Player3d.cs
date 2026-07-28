using Godot;
using System;

public partial class Player3d : CharacterBody3D
{
    //---Variables---

    // --Movement variables--
    public bool _running = false;
    public float jumpBufferTimer = 0;
    [Export] public float Speed = 4.0f;
	[Export] public float RunSpeed = 3.0f;
    [Export] private  float JumpVelocity = 4.5f;

    //---Constants---
    private const float jumpBufferTime = 0.2f;
    public const float MouseSensitivity = 0.003f;
    private const float groundAccel = 40f;
    private const float Friction = 35.0f;
    private const float MaxPitchAngle = 85.0f;

    //---NodeReferences---
    private Marker3D _twistPivot;
    private Marker3D _pitchPivot;
    private RayCast3D _interactRay;

    //---Interaction state---
    private IInteractable _focusedInteractable; // what the player is currently looking at
    private IInteractable _heldInteractable;    // what the player is currently holding/dragging

    public override void _Ready()
    {
        _twistPivot = GetNode<Marker3D>("CamPivot");
        _pitchPivot = GetNode<Marker3D>("CamPivot/NeckPivot");
        _interactRay = GetNode<RayCast3D>("CamPivot/NeckPivot/Camera3D/RayCast3D");
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;
		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}
        if (Input.IsActionJustPressed("movement_action_jump"))
        {
            jumpBufferTimer = jumpBufferTime;
            if(IsOnFloor())
            {
                velocity.Y = JumpVelocity;
                jumpBufferTimer = 0;
            }
        }
        if (IsOnFloor() && jumpBufferTimer > 0.0f)
        {
            jumpBufferTimer = 0;
            velocity.Y = JumpVelocity;
        }
        // Get the input direction and handle the movement/deceleration.
		Vector2 inputDir = Input.GetVector("movement_left", "movement_right", "movement_forward", "movement_backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (IsOnFloor())
        {
            if (direction != Vector3.Zero)
            {
                velocity.X = Mathf.MoveToward(velocity.X, direction.X * (Speed + (RunSpeed * Convert.ToInt32(_running))), groundAccel * (float)delta);
                velocity.Z = Mathf.MoveToward(velocity.Z, direction.Z * (Speed + (RunSpeed * Convert.ToInt32(_running))), groundAccel * (float)delta);
            }
            else
            {
                velocity.X = Mathf.MoveToward(velocity.X, 0, Friction * (float)delta);
                velocity.Z = Mathf.MoveToward(velocity.Z, 0, Friction * (float)delta);
            }
        }
        Velocity = velocity;
		MoveAndSlide();

        UpdateFocusedInteractable();
    }

    private void UpdateFocusedInteractable()
    {
        // Don't let the raycast steal focus away from whatever's currently being held.
        if (_heldInteractable != null) return;

        _focusedInteractable = _interactRay.IsColliding()
            ? _interactRay.GetCollider() as IInteractable
            : null;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Process mouse movement if the cursor is locked
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            if (_heldInteractable != null)
            {
                // While holding a control (e.g. a lever), route mouse movement to it
                // instead of spinning the camera - otherwise aiming and pulling the
                // lever fight each other.
                _heldInteractable.OnInteractHeld(this, mouseMotion.Relative);
            }
            else
            {
                RotateY(-mouseMotion.Relative.X * MouseSensitivity);
                // 2. Rotate the camera pitch up and down (X axis)
                _pitchPivot.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);

                // 3. Clamp the vertical looking angle to prevent flipping completely upside down
                Vector3 currentRotation = _pitchPivot.Rotation;
                currentRotation.X = Mathf.Clamp(
                    currentRotation.X,
                    Mathf.DegToRad(-MaxPitchAngle),
                    Mathf.DegToRad(MaxPitchAngle)
                );
                _pitchPivot.Rotation = currentRotation;
            }
        }

        // handle sprint action being held 
		if (@event.IsActionPressed("movement_action_sprint")){_running = true;}
		// handle sprint action being unheld 
		if (@event.IsActionReleased("movement_action_sprint")){_running = false;}

        // handle grabbing/releasing whatever the player is currently looking at
        if (@event.IsActionPressed("interact"))
        {
            if (_focusedInteractable != null)
            {
                _heldInteractable = _focusedInteractable;
                _heldInteractable.OnInteractStart(this);
            }
        }
        if (@event.IsActionReleased("interact"))
        {
            if (_heldInteractable != null)
            {
                _heldInteractable.OnInteractEnd(this);
                _heldInteractable = null;
            }
        }
    }
}