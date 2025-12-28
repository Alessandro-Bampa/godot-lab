using Godot;
using System;

public partial class CrouchingState : PlayerState
{
    private void _on_crouching_state_physics_processing(double delta)
    {
        PlayerController.CameraController.UpdateCameraHeight(delta, -1);

        if (!Input.IsActionPressed("crouch") && PlayerController.IsOnFloor() && !PlayerController.CrouchCheck.IsColliding())
        {
            PlayerController.StateChart.SendEvent("onStanding");
        }
    }

    private void _on_crouching_state_entered()
    {
        PlayerController.Crouch();
    }

}
