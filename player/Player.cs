using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export]
    public float Speed = 5.0f;

    [Export]
    public float JumpVelocity = 4.5f;

    // Sensibilità del mouse
    [Export]
    public float MouseSensitivity = 0.003f;

    // Riferimento alla Camera3D (da trascinare nell'Inspector)
    [Export]
    public Camera3D CameraNode;

    public override void _Ready()
    {
        // All'avvio, cattura il mouse
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // 1. Gestione rotazione visuale (solo se il mouse è catturato)
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            // Ruota il CORPO del giocatore a destra/sinistra (Asse Y)
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);

            // Ruota la TELECAMERA su/giù (Asse X)
            if (CameraNode != null)
            {
                CameraNode.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);

                // Limita la rotazione per non spezzare il collo (-90° a +90°)
                Vector3 cameraRot = CameraNode.Rotation;
                cameraRot.X = Mathf.Clamp(cameraRot.X, Mathf.DegToRad(-90f), Mathf.DegToRad(90f));
                CameraNode.Rotation = cameraRot;
            }
        }

        // 2. Gestione liberazione mouse (tasto ESC o ui_cancel)
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        // 3. Gestione ri-cattura mouse (clic nella finestra)
        // Se si clicca un tasto del mouse e il cursore è visibile, lo catturiamo di nuovo
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            if (Input.MouseMode == Input.MouseModeEnum.Visible)
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
                // Impostiamo l'input come gestito per evitare che il clic spari subito un colpo
                GetViewport().SetInputAsHandled();
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Gravità
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        // Salto
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        // Movimento WASD
        Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        // Direzione relativa a dove sta guardando il personaggio
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}