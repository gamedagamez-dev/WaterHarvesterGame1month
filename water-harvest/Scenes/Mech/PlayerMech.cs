using Godot;

public partial class PlayerMech : CharacterBody3D
{
    [Export] public float MaxSpeed = 10.0f;
    [Export] public float Acceleration = 6.0f;

    // Radians/sec of turn rate at full rotator lever deflection.
    [Export] public float TurnSpeed = 1.5f;

    // -1 (full reverse) .. 1 (full forward).
    private float _throttle = 0f;

    // -1 (full left) .. 1 (full right).
    private float _rotator = 0f;

    // How much this body rotated around Y last physics tick. Anything standing on the
    // mech's floor (e.g. the player) can read this to turn along with it, since Godot's
    // built-in moving-platform support only carries linear velocity, not rotation.
    public float LastYawDelta { get; private set; }

    public void SetThrottle(float value)
    {
        _throttle = Mathf.Clamp(value, -1f, 1f);
    }
    public void SetRotator(float value)
    {
        _rotator = Mathf.Clamp(value, -1f, 1f);
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        // Turn rate scales with delta (frame-rate independent) and TurnSpeed, instead of
        // rotating by a full radian-per-tick at max deflection.
        LastYawDelta = -_rotator * TurnSpeed * (float)delta;
        RotateY(LastYawDelta);

        Vector3 forward = -Transform.Basis.Z;
        Vector3 targetVelocity = forward * (_throttle * MaxSpeed);

        velocity.X = Mathf.MoveToward(velocity.X, targetVelocity.X, Acceleration * (float)delta);
        velocity.Z = Mathf.MoveToward(velocity.Z, targetVelocity.Z, Acceleration * (float)delta);

        Velocity = velocity;
        MoveAndSlide();
    }
}