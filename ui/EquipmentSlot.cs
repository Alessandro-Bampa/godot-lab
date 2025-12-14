using Godot;

public partial class EquipmentSlot : Control
{
    [Export] public string SlotType { get; set; }
    public InventoryUIManager UIManager { get; set; }

    // Per sapere cosa stiamo trascinando
    public ItemData EquippedItem { get; private set; }

    private TextureRect _iconRect;

    public override void _Ready()
    {
        _iconRect = GetNode<TextureRect>("Icon");
        // Verifica iniziale se esiste già un oggetto nei dati
        if (GameSession.Instance != null && GameSession.Instance.ActiveData.Equipment.ContainsKey(SlotType))
        {
            RefreshVisual(GameSession.Instance.ActiveData.Equipment[SlotType]);
        }
    }

    public void RefreshVisual(ItemData item)
    {
        EquippedItem = item; // Salviamo il riferimento

        if (_iconRect == null) return;

        if (item != null)
        {
            _iconRect.Texture = item.Icon;
            _iconRect.Visible = true;
            TooltipText = item.Name;
        }
        else
        {
            _iconRect.Texture = null;
            _iconRect.Visible = false;
            TooltipText = "";
        }
    }

    // --- NUOVO: Permette di trascinare FUORI dallo slot ---
    public override Variant _GetDragData(Vector2 atPosition)
    {
        // Se non c'è nulla equipaggiato, non trascinare
        if (EquippedItem == null) return default;

        // Crea anteprima visiva
        var preview = new TextureRect();
        preview.Texture = EquippedItem.Icon;
        preview.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        preview.Size = new Vector2(40, 40); // Dimensione fissa anteprima
        preview.Modulate = new Color(1, 1, 1, 0.5f);
        SetDragPreview(preview);

        // Prepara i dati per la griglia
        var dragInfo = new InventoryGridUI.DragDataInfo
        {
            ItemInstance = null,          // Non è in una griglia
            OriginalInventory = null,     // Non ha un inventario griglia
            SourceEquipmentSlot = this    // Riferimento a questo slot
        };

        return dragInfo;
    }

    // Ricezione Drop (Invariato)
    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        var dragInfo = data.Obj as InventoryGridUI.DragDataInfo;
        return dragInfo != null;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var dragInfo = data.Obj as InventoryGridUI.DragDataInfo;
        if (dragInfo == null) return;

        // Determina quale item stiamo ricevendo
        ItemData incomingItem = null;
        if (dragInfo.ItemInstance != null) incomingItem = dragInfo.ItemInstance.SourceItem;
        else if (dragInfo.SourceEquipmentSlot != null) incomingItem = dragInfo.SourceEquipmentSlot.EquippedItem;

        if (incomingItem == null) return;

        // 1. Rimuovi dalla vecchia posizione
        if (dragInfo.OriginalInventory != null)
        {
            dragInfo.OriginalInventory.RemoveItem(dragInfo.ItemInstance);
        }
        else if (dragInfo.SourceEquipmentSlot != null)
        {
            InventoryManager.Instance.UnequipItem(dragInfo.SourceEquipmentSlot.SlotType);
        }

        // 2. Equipaggia qui
        InventoryManager.Instance.EquipItem(SlotType, incomingItem);
    }
}