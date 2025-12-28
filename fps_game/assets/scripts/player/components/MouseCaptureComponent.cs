using Godot;
using System;

public partial class MouseCaptureComponent : Node
{
    [Export] bool Debug = false;
    [ExportCategory("Mouse Capture Settings")]
    [Export] Input.MouseModeEnum CurrentMouseMode = Input.MouseModeEnum.Captured;
    [Export] float mouseSensitivity = 0.005f;

    private bool _captureMouse;
    public Vector2 MouseInput;

    public override void _UnhandledInput(InputEvent @event)
    {
        // Quando il mouse si muove emette questo tipo di evento evento che vogliamo catturare
        _captureMouse = @event is InputEventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured;

        // 
        if(_captureMouse) 
        {
            var mouseMotion = (InputEventMouseMotion)@event;
            MouseInput += -mouseMotion.ScreenRelative * mouseSensitivity;
        }
        if (Debug) GD.Print(MouseInput);
    }

    public override void _Ready()
    {
        Input.MouseMode = CurrentMouseMode;
    }

}
