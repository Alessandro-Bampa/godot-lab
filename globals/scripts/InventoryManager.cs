using Godot;
using Godot.Collections;
public partial class InventoryManager : Node
{
    public static InventoryManager Instance { get; private set; }

    [Signal] 
    public delegate void EquipmentUpdatedEventHandler(string slotName, ItemData item);

    [Signal] 
    public delegate void InventoryToggledEventHandler(bool isOpen);

    [Signal]
    public delegate void InventoryUpdatedEventHandler();

    [Export] public string InputInventory = "inventory";

    [Export] public InventoryUIManager UIManager;

    private bool _isOpen;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed(InputInventory))
        {
            ToggleInventory();
        }

        if (@event.IsActionPressed("ui_refresh")) // Mappa un tasto save rapido
        {
            GameSession.Instance.SaveGame();
        }
    }

    public void ToggleInventory()
    {
        _isOpen = !_isOpen;

        // Gestione Pausa Gioco
        //GetTree().Paused = _isOpen;

        // Gestione Mouse
        Input.MouseMode = _isOpen ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;

        EmitSignal(SignalName.InventoryToggled, _isOpen);

        if (_isOpen)
        {
            RefreshUI();
        }
    }

    // Chiamato per forzare un ricaricamento totale della UI
    public void RefreshUI()
    {
        var data = GameSession.Instance.ActiveData;
        if (data == null) return;

        // 1. Aggiorna gli Slot Equipaggiamento
        foreach (var slot in data.Equipment.Keys)
        {
            // Ora questo metodo esisterà nella UI
            UIManager.UpdateEquipmentSlot(slot, data.Equipment[slot]);
        }

        // 2. Aggiorna le Tasche (Pockets)
        UIManager.UpdateContainerPanel("Pockets", data.Pockets);
    }

    public void EquipItem(string slotName, ItemData item)
    {
        GameSession.Instance.ActiveData.Equipment[slotName] = item;

        // Emetti segnale (per chi ascolta)
        EmitSignal(SignalName.EquipmentUpdated, slotName, item);

        // Emetti segnale generico di inventario aggiornato
        EmitSignal(SignalName.InventoryUpdated);

        if (_isOpen) RefreshUI();
    }

    public void UnequipItem(string slotName)
    {
        if (GameSession.Instance.ActiveData.Equipment.ContainsKey(slotName))
        {
            GameSession.Instance.ActiveData.Equipment.Remove(slotName);
            EmitSignal(SignalName.EquipmentUpdated, slotName, (ItemData)null);
            EmitSignal(SignalName.InventoryUpdated);

            if (_isOpen) RefreshUI();
        }
    }


}
