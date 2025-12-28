using Godot;
using System;

public partial class CameraController : Node3D
{
    [Export] bool Debug = false;

    [ExportCategory("References")]
    [Export] 
    public PlayerController PlayerController;
    [Export]
    MouseCaptureComponent ComponentMouseCapture;

    [ExportCategory("Camera Settings")]
    [ExportGroup("Camera Tilt")]
    [Export(PropertyHint.Range, "-90, -60")]
    public int TiltLowerLimit = -90;
    [Export(PropertyHint.Range, "60, 90")]
    public int TiltUpperLimit = 90;

    [ExportGroup("Export Vertical Movement")]
    [Export] public float CrouchOffset = 0.0f;
    [Export] public float DefaultHeight = 0.5f;
    [Export] public float CrouchSpeed = 3.0f;

    private Vector3 _rotation;

    public override void _Process(double delta)
    {

        Vector2 mouseInput = ComponentMouseCapture.MouseInput;

        UpdateCameraRotation(ComponentMouseCapture.MouseInput);

        ComponentMouseCapture.MouseInput = Vector2.Zero;
    }

    public void UpdateCameraRotation(Vector2 input)
    {
        _rotation.X += input.Y;
        _rotation.Y += input.X;
        _rotation.X = Mathf.Clamp(_rotation.X, Mathf.DegToRad(TiltLowerLimit), Mathf.DegToRad(TiltUpperLimit));

        Vector3 playerRotation = new Vector3(0.0f, _rotation.Y, 0.0f);
        Vector3 cameraRotation = new Vector3(_rotation.X, 0.0f, 0.0f);

        Rotation = cameraRotation;

        if (PlayerController != null)
        {
            PlayerController.UpdateRotation(playerRotation);
        }

        Rotation = new Vector3(Rotation.X, Rotation.Y, 0.0f);
    }

    public void UpdateCameraHeight(double delta, int direction)
    {
        if (Position.Y >= CrouchOffset && Position.Y <= DefaultHeight) 
        {
            float newY = Position.Y + (CrouchSpeed * direction) * (float)delta;
            newY = Mathf.Clamp(newY, CrouchOffset, DefaultHeight);
            Position = new Vector3(Position.X, newY, Position.Z);
        }
    }
}
