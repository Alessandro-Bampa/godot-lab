using Godot;
using System;

public partial class StandingState : PlayerState
{
    private void _on_standing_state_physics_processing(double delta)
    {
        PlayerController.CameraController.UpdateCameraHeight(delta, 1);

        if(Input.IsActionPressed("crouch") && PlayerController.IsOnFloor())
        {
            GD.Print("GO Crouching");
            PlayerController.StateChart.SendEvent("onCrouching");
        }
    }

    private void _on_standing_state_entered()
    {
        PlayerController.Stand();
    }
}
