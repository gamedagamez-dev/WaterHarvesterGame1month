using Godot;

public partial class ControlPanel : StaticBody3D
{
    // Which lever on this panel acts as the mech's throttle. Point this at a different
    // lever if you rearrange the panel's nodes, or duplicate the Lever node for other
    // controls (weapons, doors, etc.) and simply don't wire them up here.
    [Export] public NodePath ThrottleLeverPath = new NodePath("HingeJoint3D/RigidBody3D");

    private Lever _throttleLever;
    private PlayerMech _mech;

    public override void _Ready()
    {
        _mech = GetParent() as PlayerMech;
        if (_mech == null)
        {
            GD.PushWarning($"{Name}: ControlPanel expects a PlayerMech as its parent.");
        }

        if (ThrottleLeverPath != null )
        {
            _throttleLever = GetNodeOrNull<Lever>(ThrottleLeverPath);
        }

        if (_throttleLever != null)
        {
            _throttleLever.ValueChanged += OnThrottleLeverValueChanged;
            OnThrottleLeverValueChanged(_throttleLever.Value); // sync mech to the lever's starting position
        }
        else
        {
            GD.PushWarning($"{Name}: no throttle lever found at '{ThrottleLeverPath}'.");
        }
    }

    private void OnThrottleLeverValueChanged(float value)
    {
        _mech?.SetThrottle(value);
    }
}