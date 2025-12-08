using Godot;
using System;

// ProtoController v1.0 by Brackeys - Translated to C#
public partial class ProtoController : CharacterBody3D
{
    // Can we move around?
    [Export] public bool CanMove = true;
    // Are we affected by gravity?
    [Export] public bool HasGravity = true;
    // Can we press to jump?
    [Export] public bool CanJump = true;
    // Can we hold to run?
    [Export] public bool CanSprint = false;
    // Can we press to enter freefly mode (noclip)?
    [Export] public bool CanFreefly = false;

    [ExportGroup("Speeds")]
    [Export] public float LookSpeed = 0.002f;
    // Normal speed.
    [Export] public float BaseSpeed = 7.0f;
    // Speed of jump.
    [Export] public float JumpVelocity = 4.5f;
    // How fast do we run?
    [Export] public float SprintSpeed = 10.0f;
    // How fast do we freefly?
    [Export] public float FreeflySpeed = 25.0f;

    [ExportGroup("Input Actions")]
    // Input Action Names
    [Export] public string InputLeft = "ui_left";
    [Export] public string InputRight = "ui_right";
    [Export] public string InputForward = "ui_up";
    [Export] public string InputBack = "ui_down";
    [Export] public string InputJump = "jump";
    [Export] public string InputSprint = "sprint";
    [Export] public string InputFreefly = "freefly";
    [Export] public string InputInteract = "interact";


    private bool _mouseCaptured = false;
    private Vector2 _lookRotation;
    private float _moveSpeed = 0.0f;
    private bool _freeflying = false;

    // References corresponding to @onready in GDScript
    private Node3D _head;
    private CollisionShape3D _collider;
    private RayCast3D _raycast;

    public override void _Ready()
    {
        // Getting nodes manually since C# doesn't have @onready
        _head = GetNode<Node3D>("Head");
        _collider = GetNode<CollisionShape3D>("Collider");
        _raycast = GetNode<RayCast3D>("Head/Camera3D/RayCast3D");

        CheckInputMappings();

        // Initialize look rotation based on current transforms
        _lookRotation.Y = Rotation.Y;
        _lookRotation.X = _head.Rotation.X;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // Mouse capturing
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            CaptureMouse();
        }
        if (Input.IsKeyPressed(Key.Escape))
        {
            ReleaseMouse();
        }

        // Look around
        if (_mouseCaptured && @event is InputEventMouseMotion mouseMotion)
        {
            RotateLook(mouseMotion.Relative);
        }

        // Toggle freefly mode
        if (CanFreefly && Input.IsActionJustPressed(InputFreefly))
        {
            if (!_freeflying)
                EnableFreefly();
            else
                DisableFreefly();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Cast delta to float for Vector math simplicity
        float fDelta = (float)delta;

        RayCastChecks();

        // If freeflying, handle freefly and nothing else
        if (CanFreefly && _freeflying)
        {
            Vector2 inputDir = Input.GetVector(InputLeft, InputRight, InputForward, InputBack);
            // Logic for freefly motion relative to head
            Vector3 motion = (_head.GlobalBasis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

            motion *= FreeflySpeed * fDelta;
            MoveAndCollide(motion);
            return;
        }

        Vector3 velocity = Velocity;

        // Apply gravity
        if (HasGravity)
        {
            if (!IsOnFloor())
            {
                velocity += GetGravity() * fDelta;
            }
        }

        // Apply jumping
        if (CanJump)
        {
            if (Input.IsActionJustPressed(InputJump) && IsOnFloor())
            {
                velocity.Y = JumpVelocity;
            }
        }

        // Modify speed based on sprinting
        if (CanSprint && Input.IsActionPressed(InputSprint))
        {
            _moveSpeed = SprintSpeed;
        }
        else
        {
            _moveSpeed = BaseSpeed;
        }

        // Apply desired movement logic
        if (CanMove)
        {
            Vector2 inputDir = Input.GetVector(InputLeft, InputRight, InputForward, InputBack);
            // Transform.Basis is used for local movement relative to body
            Vector3 moveDir = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

            if (moveDir != Vector3.Zero)
            {
                velocity.X = moveDir.X * _moveSpeed;
                velocity.Z = moveDir.Z * _moveSpeed;
            }
            else
            {
                velocity.X = Mathf.MoveToward(velocity.X, 0, _moveSpeed);
                velocity.Z = Mathf.MoveToward(velocity.Z, 0, _moveSpeed);
            }
        }
        else
        {
            velocity.X = 0;
            // Note: We don't reset Y here to allow gravity to keep working if movement is disabled
            // Matches original script logic where gravity is applied before this block
            if (!HasGravity) velocity.Y = 0;
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    // Rotate look logic
    private void RotateLook(Vector2 rotInput)
    {
        _lookRotation.X -= rotInput.Y * LookSpeed;
        _lookRotation.X = Mathf.Clamp(_lookRotation.X, Mathf.DegToRad(-85), Mathf.DegToRad(85));
        _lookRotation.Y -= rotInput.X * LookSpeed;

        // Apply rotations
        Basis = Basis.Identity;
        RotateY(_lookRotation.Y);

        _head.Basis = Basis.Identity;
        _head.RotateX(_lookRotation.X);
    }

    private void EnableFreefly()
    {
        _collider.Disabled = true;
        _freeflying = true;
        Velocity = Vector3.Zero;
    }

    private void DisableFreefly()
    {
        _collider.Disabled = false;
        _freeflying = false;
    }

    private void CaptureMouse()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        _mouseCaptured = true;
    }

    private void ReleaseMouse()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        _mouseCaptured = false;
    }

    // Check Input Mappings
    private void CheckInputMappings()
    {
        if (CanMove && !InputMap.HasAction(InputLeft))
        {
            GD.PushError("Movement disabled. No InputAction found for InputLeft: " + InputLeft);
            CanMove = false;
        }
        if (CanMove && !InputMap.HasAction(InputRight))
        {
            GD.PushError("Movement disabled. No InputAction found for InputRight: " + InputRight);
            CanMove = false;
        }
        if (CanMove && !InputMap.HasAction(InputForward))
        {
            GD.PushError("Movement disabled. No InputAction found for InputForward: " + InputForward);
            CanMove = false;
        }
        if (CanMove && !InputMap.HasAction(InputBack))
        {
            GD.PushError("Movement disabled. No InputAction found for InputBack: " + InputBack);
            CanMove = false;
        }
        if (CanJump && !InputMap.HasAction(InputJump))
        {
            GD.PushError("Jumping disabled. No InputAction found for InputJump: " + InputJump);
            CanJump = false;
        }
        if (CanSprint && !InputMap.HasAction(InputSprint))
        {
            GD.PushError("Sprinting disabled. No InputAction found for InputSprint: " + InputSprint);
            CanSprint = false;
        }
        if (CanFreefly && !InputMap.HasAction(InputFreefly))
        {
            GD.PushError("Freefly disabled. No InputAction found for InputFreefly: " + InputFreefly);
            CanFreefly = false;
        }

    }

    private void RayCastChecks()
    {
        if (Input.IsActionJustPressed(InputInteract))
        {
            if (_raycast.IsColliding())
            {
                // Otteniamo il nodo colpito fisicamente (es. lo StaticBody3D)
                // Lo castiamo a Node perché GetCollider restituisce un GodotObject
                Node currentNode = _raycast.GetCollider() as Node;

                //Ciclo While per risalire la parentela
                while (currentNode != null)
                {
                    // Controllo: Questo nodo ha l'interfaccia?
                    if (currentNode is IInteractable interactable)
                    {
                        GD.Print($"Interazione trovata su: {currentNode.Name}");
                        interactable.Interact();
                        return;
                    }

                    // Se non è interagibile, passiamo al padre e ripetiamo il ciclo
                    currentNode = currentNode.GetParent();

                    // Opzionale: Se arriviamo alla root della scena principale, ci fermiamo
                    // per evitare di cercare all'infinito o uscire dal livello
                    if (currentNode == GetTree().CurrentScene) break;
                }

                GD.Print("Colpito qualcosa, ma nessun IInteractable trovato nei genitori.");
            }
        }
    }
}
