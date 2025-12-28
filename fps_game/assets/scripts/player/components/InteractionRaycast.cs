using Godot;
using System;

public partial class InteractionRaycast : RayCast3D
{
    public GodotObject CurrentObject;
    public override void _Process(double delta)
    {
        if (IsColliding())
        {
            var collider = GetCollider(); 
            if (collider == CurrentObject)
            {
                return;
            }
            else
            {
                CurrentObject = collider;
            }
        }
        else
        {
            CurrentObject = null;
        }
    }
}
