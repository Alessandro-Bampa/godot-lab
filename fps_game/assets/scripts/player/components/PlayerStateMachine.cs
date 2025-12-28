using Godot;
using System;

public partial class PlayerStateMachine : Node
{

    [Export] 
    public bool Debug = false;

    [ExportCategory("References")]
    [Export]
    public PlayerController PlayerController;

    public override void _Process(double delta)
    {
        // State machine debug extra informations
        if (PlayerController != null)
        {
            PlayerController.StateChart.SetExpressionProperty("Player Velocity", PlayerController.Velocity);
            PlayerController.StateChart.SetExpressionProperty("Player Hitting Head", PlayerController.CrouchCheck.IsColliding());
            PlayerController.StateChart.SetExpressionProperty("Looking At: ", PlayerController.InteractionRaycast.GetCollider());
        }
    }
}
