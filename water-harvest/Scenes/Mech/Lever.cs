using Godot;

// A generic, reusable, physically simulated lever.
//
// It doesn't know what it controls - it just reports its own position (driven by real
// physics through the HingeJoint3D it sits under) as a normalized value via the
// ValueChanged signal. Duplicate this node anywhere you need a physical control -
// throttle, weapon select, a valve, a door switch - and connect ValueChanged to whatever
// should react to it. See ControlPanel.cs for an example listener (the mech throttle).
//
// Expected setup (already true of the HingeJoint3D/RigidBody3D pair in controlPanel.tscn):
//   - This script is on a RigidBody3D.
//   - Its direct parent is a HingeJoint3D with motor/enable = true.
//   - The HingeJoint3D has angular_limit/enable = true with lower/upper set.
//   - Exactly one angular axis is left unlocked on this RigidBody3D (axis_lock_angular_*);
//     RotationAxis below must match that axis.
public partial class Lever : RigidBody3D, IInteractable
{
    // ---Configuration---
    // Match these to the HingeJoint3D's angular_limit/lower and /upper (in degrees) so the
    // reported value actually reaches -1 / 1 right at the physical end-stops.
    [Export] public float MinAngleDegrees = -30f;
    [Export] public float MaxAngleDegrees = 30f;

    // Which local rotation axis is the free one - must match whichever angular axis is
    // NOT locked via axis_lock_angular_x/y/z on this body.
    [Export] public Vector3.Axis RotationAxis = Vector3.Axis.Z;

    // Degrees/sec of motor speed applied per pixel of mouse movement while dragging.
    [Export] public float DragSensitivity = 0.6f;
    [Export] public bool InvertDrag = false;
    [Export] public string InteractionPrompt = "Grab Lever";

    // ---Signals---
    // Fired whenever the lever's value changes. Listeners decide what the value means.
    [Signal] public delegate void ValueChangedEventHandler(float value);

    // ---Public state---
    // -1 (min limit) .. 1 (max limit), 0 = centered. This is what most listeners want
    // (e.g. a throttle: negative = reverse, positive = forward).
    public float Value { get; private set; }
    // 0 (min limit) .. 1 (max limit), for controls where that reads more naturally
    // (e.g. a valve that's just "how open is it").
    public float Value01 { get; private set; }

    private HingeJoint3D _hinge;
    private float _lastEmittedValue = float.NaN;

    public override void _Ready()
    {
        _hinge = GetParent() as HingeJoint3D;
        if (_hinge == null)
        {
            GD.PushWarning($"{Name}: Lever expects its parent node to be a HingeJoint3D.");
        }
        else
        {
            _hinge.SetFlag(HingeJoint3D.Flag.EnableMotor, true);
            _hinge.SetParam(HingeJoint3D.Param.MotorTargetVelocity, 0f);
        }

        RecalculateValue();
        _lastEmittedValue = Value;
    }

    public override void _PhysicsProcess(double delta)
    {
        RecalculateValue();

        if (Mathf.Abs(Value - _lastEmittedValue) > 0.0015f)
        {
            _lastEmittedValue = Value;
            EmitSignal(SignalName.ValueChanged, Value);
        }
    }

    private void RecalculateValue()
    {
        float angleDeg = Mathf.RadToDeg(GetAxisRotation());
        Value01 = Mathf.Clamp(Mathf.InverseLerp(MinAngleDegrees, MaxAngleDegrees, angleDeg), 0f, 1f);
        Value = Value01 * 2f - 1f;
    }

    private float GetAxisRotation()
    {
        Vector3 rot = Rotation;
        return RotationAxis switch
        {
            Vector3.Axis.X => rot.X,
            Vector3.Axis.Y => rot.Y,
            _ => rot.Z,
        };
    }

    // ---IInteractable---
    public string GetInteractionPrompt() => InteractionPrompt;

    public void OnInteractStart(Node3D interactor)
    {
        _hinge?.SetFlag(HingeJoint3D.Flag.EnableMotor, true);
    }

    public void OnInteractHeld(Node3D interactor, Vector2 mouseDelta)
    {
        if (_hinge == null) return;

        // Mouse up (negative Relative.Y) drives the lever toward MaxAngleDegrees.
        float sign = InvertDrag ? -1f : 1f;
        float targetVelocityDegPerSec = sign * -mouseDelta.Y * DragSensitivity;
        _hinge.SetParam(HingeJoint3D.Param.MotorTargetVelocity, Mathf.DegToRad(targetVelocityDegPerSec));
    }

    public void OnInteractEnd(Node3D interactor)
    {
        // Zero target velocity turns the motor into a brake (up to motor/max_impulse),
        // so the lever stays roughly where it was left instead of drooping under gravity.
        _hinge?.SetParam(HingeJoint3D.Param.MotorTargetVelocity, 0f);
    }
}