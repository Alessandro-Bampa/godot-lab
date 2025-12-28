using Godot;
using GodotStateCharts;
using System;

public partial class PlayerController : CharacterBody3D
{
    [Export]
    public bool Debug = false;

    [Export]
    public CameraController CameraController;

    [Export]
    private Node StateChartNode;

    [Export]
    public CollisionShape3D StandingCollision;

    [Export]
    public CollisionShape3D CrouchCollision;

    [Export]
    public ShapeCast3D CrouchCheck;

    [Export]
    public RayCast3D InteractionRaycast;

    [Export]
    public float Acceleration = 0.2f; // Valore tra 0 e 1 per Lerp

    [Export]
    public float Deceleration = 0.5f; // Valore di rallentamento

    // Variabili esportate (Settings)
    [Export]
    public float DefaultSpeed = 7.0f; // Ho messo 5 come default sensato

    [Export]
    public float SprintSpeed = 3.0f;

    [Export]
    public float CrouchSpeed = -0.5f;

    [ExportCategory("Jump Settings")]
    [Export]
    public float JumpVelocity = 5.0f;


    // Variabili interne
    public Vector2 _inputDir = Vector2.Zero;
    public Vector3 _movementVelocity = Vector3.Zero;

    public float SprintModifier = 0.0f;
    public float CrouchModifier = 0.0f;
    public float Speed = 0.0f;

    // Recuperiamo la gravità dalle impostazioni del progetto
    public float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public StateChart StateChart { get; private set; }

    public override void _Ready()
    {
        if (StateChartNode != null)
        {
            StateChart = StateChart.Of(StateChartNode);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Copiamo la Velocity attuale in una variabile locale modificabile
        Vector3 velocity = Velocity;

        // 1. Applicazione Gravità
        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
        }

        var speedModifier = SprintModifier + CrouchModifier;
        Speed = DefaultSpeed + speedModifier;

        // 2. Input
        _inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        // 3. Calcolo velocità orizzontale corrente (ignoriamo Y per ora)
        // Nota: Qui usiamo _movementVelocity.Z mappato su Y del Vector2, come nel tuo script
        var currentVelocity = new Vector2(_movementVelocity.X, _movementVelocity.Z);

        // 4. Calcolo Direzione (Relativa alla rotazione del player)
        // Transform.Basis moltiplica il vettore di input per ruotarlo
        var direction = (Transform.Basis * new Vector3(_inputDir.X, 0, _inputDir.Y)).Normalized();

        // 5. Logica Movimento (Accelerazione / Decelerazione)
        // In C# non puoi scrivere "if (direction)". Devi controllare se la lunghezza è > 0
        if (direction != Vector3.Zero)
        {
            // Convertiamo la direzione 3D in 2D per il calcolo
            var direction2D = new Vector2(direction.X, direction.Z);

            // Lerp per accelerare gradualmente
            currentVelocity = currentVelocity.Lerp(direction2D * Speed, Acceleration);
        }
        else
        {
            // MoveToward per fermarsi
            currentVelocity = currentVelocity.MoveToward(Vector2.Zero, Deceleration);
        }

        // 6. Riassemblaggio velocità finale
        // Combiniamo la velocità orizzontale calcolata (X, Y del Vector2) 
        // con la velocità verticale originale (Gravity)
        _movementVelocity = new Vector3(currentVelocity.X, velocity.Y, currentVelocity.Y);

        // Assegniamo alla proprietà Velocity principale e muoviamo
        Velocity = _movementVelocity;
        MoveAndSlide();
    }

    public void UpdateRotation(Vector3 rotationInput)
    {
        // Assegnare direttamente GlobalRotation aggiorna automaticamente la Basis
        GlobalRotation = rotationInput;
    }

    public void Sprint()
    {
        SprintModifier = SprintSpeed;
    }

    public void Walk()
    {
        SprintModifier = 0.0f;
    }

    public void Stand()
    {
        CrouchModifier = 0.0f;
        StandingCollision.Disabled = false;
        CrouchCollision.Disabled = true;
    }

    public void Crouch()
    {
        CrouchModifier = CrouchSpeed;
        StandingCollision.Disabled = true;
        CrouchCollision.Disabled = false;
    }

    public void Jump()
    {
        Velocity = new Vector3(Velocity.X, JumpVelocity, Velocity.Z);
    }
}
