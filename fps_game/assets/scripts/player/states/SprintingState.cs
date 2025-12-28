using Godot;
using System;

public partial class SprintingState : PlayerState
{
    private void _on_sprinting_state_physics_processing(double delta)
    {
        if (!Input.IsActionPressed("sprint"))
        {
            PlayerController.StateChart.SendEvent("onWalking");
        }
    }

    private void _on_sprinting_state_entered()
    {
        PlayerController.Sprint();
    }
}
