using Godot;
using System;

public partial class PlayerState : Node
{
    [Export]
    public bool Debug = false;
    
    public PlayerController PlayerController;

    public override void _Ready()
    {
        var stateMachineNode = GetNodeOrNull("%StateMachine");

        if (stateMachineNode != null && stateMachineNode is PlayerStateMachine stateMachine)
        {
            PlayerController = stateMachine.PlayerController;
        } 
    }

}
