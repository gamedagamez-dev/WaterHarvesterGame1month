using Godot;

// Implement this on any Node3D-derived object (StaticBody3D, RigidBody3D, etc.) that the
// player should be able to interact with by looking at it and holding the "interact" input.
//
// This is the contract between the player's raycast-based interaction system (see
// Player3d.cs) and whatever is being interacted with. Levers, buttons, valves, doors, etc.
// can all implement this the same way, so the player script never needs to know about
// specific object types.
public interface IInteractable
{
    // Short prompt you could show in a UI, e.g. "Grab Lever" / "Open Door".
    string GetInteractionPrompt();

    // Called the instant the interact input is pressed while this object is focused.
    void OnInteractStart(Node3D interactor);

    // Called every input frame the interact input is held down, with the raw mouse
    // movement for that frame. Used for drag-style controls (levers, wheels, dials).
    void OnInteractHeld(Node3D interactor, Vector2 mouseDelta);

    // Called when the interact input is released.
    void OnInteractEnd(Node3D interactor);
}
