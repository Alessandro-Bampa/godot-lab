using Godot;
using System;

public partial class GameManager : Node
{
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("dev_exit"))
        {
            GetTree().Quit();
        }

        if (@event.IsActionPressed("dev_reload"))
        {
            GetTree().ReloadCurrentScene();
        }
    }
}
