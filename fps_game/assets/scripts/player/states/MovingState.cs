using Godot;
using System;

public partial class MovingState : PlayerState
{
    private void _on_moving_state_physics_processing(double delta)
    {
        if (PlayerController != null && PlayerController._inputDir.Length() == 0  && PlayerController.Velocity.Length() < 0.5)
        {
            PlayerController.StateChart.SendEvent("onIdle");
        }
    }
}
