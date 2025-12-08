using Godot;
using System;

public partial class InventoryUI : Control
{
    [Export] public Node ContainerGrid; // Assegna il GridContainer nell'inspector
    [Export] public PackedScene SlotScene; // Opzionale: se vuoi creare slot personalizzati

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;
        // Ci colleghiamo ai segnali del Manager
        InventoryManager.Instance.InventoryUpdated += UpdateUI;
        InventoryManager.Instance.InventoryToggled += OnToggle;

    }

    // Ricordati di scollegare gli eventi quando l'oggetto viene distrutto per evitare memory leaks
    public override void _ExitTree()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.InventoryUpdated -= UpdateUI;
            InventoryManager.Instance.InventoryToggled -= OnToggle;
        }
    }

    private void OnToggle(bool isOpen)
    {
        Visible = isOpen;
        // mette in pausa l'intero albero tranne gli script segnati con ProcessMode = ProcessModeEnum.Always;
        GetTree().Paused = isOpen;
        if (isOpen)
        {
            // Aggiorna quando apri per sicurezza
            UpdateUI(); // todo vedere un metodo per non dover ricaricare l'inventario ogni volta, ma non dovrebbe essere pesante "capiamo"
            Input.MouseMode = Input.MouseModeEnum.Visible;
        } else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;

        }
    }

    private void UpdateUI()
    {
        // 1. Pulisci tutto (metodo brutale ma efficace per prototipi)
        foreach (Node child in ContainerGrid.GetChildren())
        {
            child.QueueFree();
        }

        // 2. Ricrea gli slot
        foreach (var pair in GameSession.Instance.ActiveData.Inventory)
        {
            ItemData item = pair.Key;
            int amount = pair.Value;

            // Creiamo un semplice bottone o label per ora
            Button slot = new Button();
            slot.Text = $"{item.DisplayName} x{amount}";
            //slot.Icon = item.Icon; // Se hai impostato l'icona
            slot.ExpandIcon = true;
            slot.CustomMinimumSize = new Vector2(100, 40); // Dimensione fissa

            ContainerGrid.AddChild(slot);
        }
    }
}