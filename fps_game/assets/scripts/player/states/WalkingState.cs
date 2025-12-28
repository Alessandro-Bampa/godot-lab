using Godot;
using System;

public partial class WalkingState : PlayerState
{

    private void _on_walking_state_physics_processing(double delta)
    {
        if (Input.IsActionPressed("sprint"))
        {
            PlayerController.StateChart.SendEvent("onSprinting");
        }
    }

    private void _on_walking_state_entered()
    {
        PlayerController.Walk();
    }
}
