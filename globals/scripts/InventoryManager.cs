using Godot;
using Godot.Collections;
public partial class InventoryManager : Node
{
    public static InventoryManager Instance { get; private set; }

    [Signal]
    public delegate void InventoryUpdatedEventHandler();

    [Signal] 
    public delegate void InventoryToggledEventHandler(bool isOpen);

    [Export] public string InputInventory = "inventory";

    private bool _isOpen;

    public Dictionary _inventory = new() { };

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed(InputInventory))
        {
            _isOpen = !_isOpen;

            EmitSignal(SignalName.InventoryToggled, _isOpen);

            GD.Print($"Stato Inventario: {(_isOpen ? "Aperto" : "Chiuso")}");

            if (_isOpen) PrintInventoryDebug();
        }

        // salvataggio rapido
        if (Input.IsActionJustPressed("ui_refresh"))
        {
            GameSession.Instance.SaveGame();
        }
    }

    public void AddItem(ItemData item, int amount = 1)
    {
        // Accediamo ai dati persistenti tramite GameSession
        var inventory = GameSession.Instance.ActiveData.Inventory;

        if (inventory.ContainsKey(item))
        {
            inventory[item] += amount;
        }
        else
        {
            inventory[item] = amount;
        }

        GD.Print($"Aggiunto {item.DisplayName}. Ora ne hai {inventory[item]}");

        // Notifichiamo la UI
        EmitSignal(SignalName.InventoryUpdated);

        // Opzionale: Salva immediatamente ad ogni raccolta
        // GameSession.Instance.SaveGame(); 
    }

    private void PrintInventoryDebug()
    {
        var inv = GameSession.Instance.ActiveData.Inventory;
        foreach (var item in inv.Keys)
        {
            GD.Print($"- {item.DisplayName}: {inv[item]}");
        }
    }


}
