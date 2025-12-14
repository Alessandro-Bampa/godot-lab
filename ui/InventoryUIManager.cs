using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class InventoryUIManager : Control
{
    [Export] public VBoxContainer RightPanelContainer;
    [Export] public Control LeftPanelContainer;
    [Export] public InventoryGridUI VicinityGrid;
    [Export] public PackedScene GridPrefab;

    private Dictionary<string, InventoryGridUI> _activeContainerGrids = new();
    private Dictionary<string, EquipmentSlot> _equipmentSlots = new();

    public override void _Ready()
    {
        // Collegamenti base
        foreach (var child in LeftPanelContainer.GetChildren())
        {
            if (child is EquipmentSlot slot)
            {
                _equipmentSlots[slot.SlotType] = slot;
                slot.UIManager = this;
            }
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.UIManager = this;
            InventoryManager.Instance.InventoryToggled += OnInventoryToggled;
            InventoryManager.Instance.InventoryUpdated += RefreshAll;
            InventoryManager.Instance.EquipmentUpdated += OnEquipmentChanged;
        }

        Visible = false;

        // --- CODICE PER VEDERE LA GRIGLIA CENTRALE ---
        if (VicinityGrid != null)
        {
            var lootData = new InventoryData();
            lootData.GridWidth = 6;  // Larghezza griglia
            lootData.GridHeight = 4; // Altezza griglia

            // Questo fa disegnare il rettangolo grigio 6x4 al centro
            VicinityGrid.SetInventoryData(lootData);
        }
    }

    private void OnInventoryToggled(bool isOpen)
    {
        Visible = isOpen;
        if (isOpen) RefreshAll();
    }

    public void RefreshAll()
    {
        if (GameSession.Instance == null || GameSession.Instance.ActiveData == null) return;
        var data = GameSession.Instance.ActiveData;

        // 1. Aggiorna tutti gli slot equipaggiamento usando il metodo helper
        foreach (var slotName in _equipmentSlots.Keys)
        {
            ItemData item = null;
            if (data.Equipment.ContainsKey(slotName))
            {
                item = data.Equipment[slotName];
            }
            UpdateEquipmentSlot(slotName, item);
        }

        // 2. Aggiorna Tasche
        if (data.Pockets != null)
        {
            ForceCreateOrUpdateGrid("Pockets", data.Pockets, "Pockets");
        }
    }

    private void OnEquipmentChanged(string slotName, ItemData newItem)
    {
        UpdateEquipmentSlot(slotName, newItem);
    }

    // FIX: Questo è il metodo che mancava ed era chiamato da InventoryManager
    public void UpdateEquipmentSlot(string slotName, ItemData item)
    {
        // 1. Aggiorna visivamente l'icona nello slot (Left Panel)
        if (_equipmentSlots.ContainsKey(slotName))
        {
            _equipmentSlots[slotName].RefreshVisual(item);
        }

        // 2. Aggiorna la griglia a destra se è un contenitore (Right Panel)
        UpdateContainerPanel(slotName, item);
    }

    private void UpdateContainerPanel(string slotName, ItemData item)
    {
        if (item == null || item.InternalInventory == null)
        {
            RemoveContainerGrid(slotName);
            return;
        }
        ForceCreateOrUpdateGrid(slotName, item.InternalInventory, item.Name);
    }

    // Metodo pubblico per chiamate esterne (es. Tasche)
    public void UpdateContainerPanel(string containerId, InventoryData data)
    {
        ForceCreateOrUpdateGrid(containerId, data, containerId);
    }

    private void ForceCreateOrUpdateGrid(string containerId, InventoryData data, string title)
    {
        if (_activeContainerGrids.ContainsKey(containerId))
        {
            _activeContainerGrids[containerId].SetInventoryData(data);
            return;
        }

        var newGridInstance = GridPrefab.Instantiate<InventoryGridUI>();
        RightPanelContainer.AddChild(newGridInstance);
        newGridInstance.SetInventoryData(data);
        _activeContainerGrids.Add(containerId, newGridInstance);
    }

    private void RemoveContainerGrid(string containerId)
    {
        if (_activeContainerGrids.ContainsKey(containerId))
        {
            _activeContainerGrids[containerId].QueueFree();
            _activeContainerGrids.Remove(containerId);
        }
    }

    public void SetVicinityData(InventoryData lootData)
    {
        if (VicinityGrid != null) VicinityGrid.SetInventoryData(lootData);
    }
}
