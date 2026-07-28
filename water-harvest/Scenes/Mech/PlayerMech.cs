using Godot;

public partial class PlayerMech : CharacterBody3D
{
    [Export] public float MaxSpeed = 8.0f;
    [Export] public float Acceleration = 6.0f;

    // -1 (full reverse) .. 1 (full forward).
    private float _throttle = 0f;

    public void SetThrottle(float value)
    {
        _throttle = Mathf.Clamp(value, -1f, 1f);
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        Vector3 forward = -Transform.Basis.Z;
        Vector3 targetVelocity = forward * (_throttle * MaxSpeed);

        velocity.X = Mathf.MoveToward(velocity.X, targetVelocity.X, Acceleration * (float)delta);
        velocity.Z = Mathf.MoveToward(velocity.Z, targetVelocity.Z, Acceleration * (float)delta);

        Velocity = velocity;
        MoveAndSlide();
    }
}