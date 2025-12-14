using Godot;
using System.Linq;

[GlobalClass]
public partial class InventoryGridUI : Control
{
    [Export] public InventoryData InventoryData;
    [Export] public int TileSize { get; set; } = 40;
    [Export] public PackedScene ItemScene { get; set; }

    private Control _itemsContainer;

    // Classe Dati per il Trascinamento
    public partial class DragDataInfo : Godot.GodotObject
    {
        public InventoryItemInstance ItemInstance; // Se viene da una griglia
        public InventoryData OriginalInventory;    // L'inventario di provenienza

        // --- NUOVO: Supporto per Drag dagli Slot Equipaggiamento ---
        public EquipmentSlot SourceEquipmentSlot;
    }

    public override void _Ready()
    {
        _itemsContainer = GetNodeOrNull<Control>("GridItems");
        if (_itemsContainer == null)
        {
            // Fallback se non hai creato il nodo GridItems (opzionale ma consigliato)
            _itemsContainer = this;
        }

        if (InventoryData != null) SetInventoryData(InventoryData);
    }

    public void SetInventoryData(InventoryData data)
    {
        if (InventoryData != null) InventoryData.InventoryUpdated -= OnInventoryUpdated;
        InventoryData = data;

        if (InventoryData != null)
        {
            InventoryData.InventoryUpdated += OnInventoryUpdated;
            // Imposta dimensione
            CustomMinimumSize = new Vector2(InventoryData.GridWidth * TileSize, InventoryData.GridHeight * TileSize);
            // Forza il ridisegno delle righe
            QueueRedraw();
        }
        OnInventoryUpdated();
    }

    // --- NUOVO: Disegna le righe della griglia ---
    public override void _Draw()
    {
        if (InventoryData == null) return;

        // Colore delle righe (es. Nero semitrasparente)
        var gridColor = new Color(0, 0, 0, 0.5f);

        // Disegna linee verticali
        for (int x = 0; x <= InventoryData.GridWidth; x++)
        {
            DrawLine(
                new Vector2(x * TileSize, 0),
                new Vector2(x * TileSize, InventoryData.GridHeight * TileSize),
                gridColor
            );
        }

        // Disegna linee orizzontali
        for (int y = 0; y <= InventoryData.GridHeight; y++)
        {
            DrawLine(
                new Vector2(0, y * TileSize),
                new Vector2(InventoryData.GridWidth * TileSize, y * TileSize),
                gridColor
            );
        }
    }

    private void OnInventoryUpdated()
    {
        // Pulisci vecchi oggetti
        foreach (var child in _itemsContainer.GetChildren())
            child.QueueFree();

        // Ridisegna oggetti
        if (InventoryData == null) return;

        foreach (var itemInstance in InventoryData.Items)
        {
            var itemNode = ItemScene.Instantiate() as Control;
            _itemsContainer.AddChild(itemNode);

            itemNode.Position = new Vector2(itemInstance.GridX * TileSize, itemInstance.GridY * TileSize);
            itemNode.Size = new Vector2(itemInstance.SourceItem.Width * TileSize, itemInstance.SourceItem.Height * TileSize);

            var iconNode = itemNode.GetNode<TextureRect>("Icon");
            if (iconNode != null) iconNode.Texture = itemInstance.SourceItem.Icon;
        }
    }

    // --- DRAG AND DROP ---

    public override Variant _GetDragData(Vector2 atPosition)
    {
        if (InventoryData == null) return default;

        int gridX = (int)(atPosition.X / TileSize);
        int gridY = (int)(atPosition.Y / TileSize);

        var item = InventoryData.GetItemAt(gridX, gridY);
        if (item == null) return default;

        // Anteprima visuale
        var preview = new TextureRect();
        preview.Texture = item.SourceItem.Icon;
        preview.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        preview.Size = new Vector2(item.SourceItem.Width * TileSize, item.SourceItem.Height * TileSize);
        preview.Modulate = new Color(1, 1, 1, 0.5f);
        SetDragPreview(preview);

        return new DragDataInfo
        {
            ItemInstance = item,
            OriginalInventory = InventoryData
        };
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return data.Obj is DragDataInfo;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var dragInfo = data.Obj as DragDataInfo;
        if (dragInfo == null) return;

        int targetX = (int)(atPosition.X / TileSize);
        int targetY = (int)(atPosition.Y / TileSize);

        // Recupera l'ItemData sorgente (che venga dalla griglia o dallo slot equip)
        ItemData sourceItemData = dragInfo.ItemInstance != null ? dragInfo.ItemInstance.SourceItem : dragInfo.SourceEquipmentSlot?.EquippedItem;
        if (sourceItemData == null) return;

        // 1. RIMUOVI DALL'ORIGINE (Logica Condizionale)
        if (dragInfo.OriginalInventory != null)
        {
            // Caso A: Viene da un'altra griglia -> Rimuovi temporaneamente
            dragInfo.OriginalInventory.RemoveItem(dragInfo.ItemInstance);
        }
        else if (dragInfo.SourceEquipmentSlot != null)
        {
            // Caso B: Viene dall'equipaggiamento -> Non facciamo nulla ora, lo faremo a drop confermato
        }

        // 2. CONTROLLA SPAZIO
        if (InventoryData.CanPlaceItem(sourceItemData, targetX, targetY))
        {
            // Spazio libero: Piazzalo
            InventoryData.AddItem(sourceItemData, targetX, targetY);

            // Conferma rimozione dall'equipaggiamento se necessario
            if (dragInfo.SourceEquipmentSlot != null)
                InventoryManager.Instance.UnequipItem(dragInfo.SourceEquipmentSlot.SlotType);
        }
        else
        {
            // Niente spazio (o collisione): Ripristina l'originale
            if (dragInfo.OriginalInventory != null)
            {
                dragInfo.OriginalInventory.AddItem(dragInfo.ItemInstance.SourceItem, dragInfo.ItemInstance.GridX, dragInfo.ItemInstance.GridY);
            }
            // Se veniva dall'equipaggiamento e non c'è spazio, non facciamo nulla (resta equipaggiato)
        }
    }
}