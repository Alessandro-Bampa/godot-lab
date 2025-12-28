using Godot;
using System;

public partial class AirborneState : PlayerState
{
    private void _on_airborne_state_physics_processing(double delta)
    {
        if (PlayerController.IsOnFloor())
        {
            PlayerController.StateChart.SendEvent("onGrounded");
        }
    }
}
