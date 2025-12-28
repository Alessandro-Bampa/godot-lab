using Godot;
using System;

public partial class GroundedState : PlayerState
{
    private void _on_grounded_state_physics_processing(double delta)
    {
        if(Input.IsActionJustPressed("jump") && PlayerController.IsOnFloor())
        {
            PlayerController.Jump();
            PlayerController.StateChart.SendEvent("onAirborne");
        }

        if(!PlayerController.IsOnFloor())
        {
            PlayerController.StateChart.SendEvent("onAirborne");
        }
    }
}
