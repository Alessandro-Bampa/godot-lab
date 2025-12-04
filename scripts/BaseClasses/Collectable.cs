using Godot;
using System;

public abstract partial class Collectable : Node, IInteractable
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

    public void Interact()
    {
        OnCollect();
		this.QueueFree();
    }

	public abstract void OnCollect();
}
