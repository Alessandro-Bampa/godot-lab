using Godot;

public abstract partial class Openable : Node3D, IInteractable
{
    [Export]
    protected AnimationPlayer _animPlayer;

    [Export]
    protected string openAnimName = "Open";

    protected bool IsOpen = false;

    private bool _canInteract = true;

    private Timer _cooldownTimer;

    private float _animDuration;

    public override void _Ready()
    {
        GD.Print($"Openable Istanziato: {this.Name}");
        _cooldownTimer = new Timer();
        _cooldownTimer.OneShot = true;
        _cooldownTimer.Name = "AnimationTimer";
        AddChild(_cooldownTimer);
        _cooldownTimer.Timeout += OnTimerTimeout;

        // Controllo di sicurezza
        if (_animPlayer == null)
        {
            GD.PrintErr($"Attenzione: AnimationPlayer non assegnato su {Name}!");
        }
        _animDuration = GetAnimationDuration(openAnimName);

    }

    public void Interact()
    {
        if (!_canInteract)
        {
            return;
        }
        _canInteract = false;
        _cooldownTimer.Start(_animDuration + 0.05f);

        IsOpen = !IsOpen;
        if (IsOpen)
        {
            OnOpen();
        }
        else
        {
            OnClose();
        }
    }

    private void OnTimerTimeout()
    {
        _canInteract = true;
    }

    public virtual string GetInteractionText()
    {
        if (!_canInteract) return "";
        return IsOpen ? "Premi [E] per Chiudere" : "Premi [E] per Aprire";
    }

    private float GetAnimationDuration(string animName)
    {
        if (_animPlayer != null && _animPlayer.HasAnimation(animName))
        {
            return _animPlayer.GetAnimation(animName).Length;
        }
        GD.PrintErr($"Animazione '{animName}' non trovata!");
        return 1.0f;
    }

    public abstract void OnOpen();
    public abstract void OnClose();
}

