using Godot;
using System;

public partial class IdleState : PlayerState
{
    private void _on_idle_state_physics_processing(double delta)
    {
        if(PlayerController != null && PlayerController._inputDir.Length() > 0)
        {
            PlayerController.StateChart.SendEvent("onMoving");
        }
       
    }
}
