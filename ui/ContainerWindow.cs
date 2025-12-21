using Godot;

public partial class ContainerWindow : PanelContainer
{
    [Export] public PackedScene GridPrefab;

    // Segnale per dire al Manager che questa finestra si è chiusa
    [Signal] public delegate void WindowClosedEventHandler(ContainerWindow window, InventoryData data);

    // Dati interni
    public InventoryData MyData { get; private set; }

    private Control _header;
    private Label _titleLabel;
    private Button _closeButton;
    private Control _gridContainerSlot;

    private bool _isDragging = false;
    private Vector2 _dragOffset;

    public override void _Ready()
    {
        _header = GetNode<Control>("VBoxContainer/Header");
        _titleLabel = GetNode<Label>("VBoxContainer/Header/HBoxContainer/TitleLabel");
        _closeButton = GetNode<Button>("VBoxContainer/Header/HBoxContainer/CloseButton");
        _gridContainerSlot = GetNode<Control>("VBoxContainer/GridContainer");

        _closeButton.Pressed += OnClosePressed;
        _header.GuiInput += OnHeaderGuiInput;
    }

    public void Init(InventoryData data, string containerName)
    {
        MyData = data; // Salviamo il riferimento ai dati
        _titleLabel.Text = containerName;

        var newGrid = GridPrefab.Instantiate<InventoryGridUI>();
        _gridContainerSlot.AddChild(newGrid);
        newGrid.SetInventoryData(data);

        // Aspettiamo un frame affinché la UI calcoli la sua dimensione reale
        // poi blocchiamo la finestra dentro lo schermo
        CallDeferred(nameof(ClampToScreen));
    }

    private void ClampToScreen()
    {
        var viewportRect = GetViewportRect();
        var mySize = GetGlobalRect().Size; // Dimensione attuale della finestra
        var myPos = GlobalPosition;

        // Calcoli per non uscire dai bordi
        float x = Mathf.Clamp(myPos.X, 0, viewportRect.Size.X - mySize.X);
        float y = Mathf.Clamp(myPos.Y, 0, viewportRect.Size.Y - mySize.Y);

        GlobalPosition = new Vector2(x, y);
    }

    private void OnHeaderGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            _isDragging = mb.Pressed;
            if (_isDragging)
            {
                _dragOffset = GetGlobalMousePosition() - GlobalPosition;
                MoveToFront(); // Porta in primo piano quando clicchi
            }
        }
        else if (@event is InputEventMouseMotion mm && _isDragging)
        {
            // Nuova posizione proposta
            Vector2 newPos = GetGlobalMousePosition() - _dragOffset;

            // Logica Clamp DURANTE il trascinamento (opzionale, ma consigliata)
            var viewportRect = GetViewportRect();
            var mySize = Size;

            newPos.X = Mathf.Clamp(newPos.X, 0, viewportRect.Size.X - mySize.X);
            newPos.Y = Mathf.Clamp(newPos.Y, 0, viewportRect.Size.Y - mySize.Y);

            GlobalPosition = newPos;
        }
    }

    private void OnClosePressed()
    {
        // Avvisa il manager PRIMA di morire
        EmitSignal(SignalName.WindowClosed, this, MyData);
        QueueFree();
    }
}
