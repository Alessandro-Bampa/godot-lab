using Godot;
using System.Linq;

[GlobalClass]
public partial class InventoryGridUI : Control
{
    [Export] public InventoryData InventoryData;
    [Export] public int TileSize { get; set; } = 40;
    [Export] public PackedScene ItemScene { get; set; }

    private Control _itemsContainer;

    // Highlight Variables
    private Rect2I? _hoverRect = null;
    private bool _isHoverValid = false;

    // Classe Dati Trascinamento Aggiornata
    public partial class DragDataInfo : Godot.GodotObject
    {
        public InventoryItemInstance ItemInstance;
        public InventoryData OriginalInventory;
        public EquipmentSlot SourceEquipmentSlot;
        public bool IsRotated = false;
        public Control PreviewControl; // Riferimento aggiunto qui!
    }

    public override void _Ready()
    {
        _itemsContainer = GetNodeOrNull<Control>("GridItems");
        if (_itemsContainer == null) _itemsContainer = this;

        if (InventoryData != null) SetInventoryData(InventoryData);

        // Pulizia base
        MouseExited += ClearHighlight;
    }

    public override void _ExitTree()
    {
        if (InventoryData != null) InventoryData.InventoryUpdated -= OnInventoryUpdated;
        base._ExitTree();
    }

    // --- NUOVO: Pulizia robusta del Ghosting ---
    // Questo intercetta quando un Drag finisce (ovunque nello schermo) o il mouse esce
    public override void _Notification(int what)
    {
        base._Notification(what);
        if (what == NotificationDragEnd || what == NotificationMouseExit)
        {
            ClearHighlight();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_rotate") || (@event is InputEventKey k && k.Pressed && k.Keycode == Key.R))
        {
            var dragVariant = GetViewport().GuiGetDragData();

            if (dragVariant.Obj is DragDataInfo info)
            {
                // 1. Logica Rotazione
                info.IsRotated = !info.IsRotated;

                // 2. Rotazione Visuale (Usa il riferimento nel DragDataInfo!)
                if (IsInstanceValid(info.PreviewControl))
                {
                    info.PreviewControl.RotationDegrees = info.IsRotated ? 90 : 0;
                }

                // 3. FIX GHOSTING: Aggiorna la griglia SOLO se il mouse è sopra QUESTA griglia
                if (GetGlobalRect().HasPoint(GetGlobalMousePosition()))
                {
                    // Forza ricalcolo validità spazio
                    _CanDropData(GetLocalMousePosition(), dragVariant);
                }
                else
                {
                    // Se ruoto mentre sono fuori, assicuriamoci di pulire eventuali residui
                    ClearHighlight();
                }

                GetViewport().SetInputAsHandled();
            }
        }
    }

    private void ClearHighlight()
    {
        if (_hoverRect.HasValue)
        {
            _hoverRect = null;
            QueueRedraw();
        }
    }

    public void SetInventoryData(InventoryData data)
    {
        if (InventoryData != null) InventoryData.InventoryUpdated -= OnInventoryUpdated;
        InventoryData = data;

        if (InventoryData != null)
        {
            InventoryData.InventoryUpdated += OnInventoryUpdated;
            // Impostiamo dimensione minima per stabilità
            CustomMinimumSize = new Vector2(InventoryData.GridWidth * TileSize, InventoryData.GridHeight * TileSize);
            QueueRedraw();
        }
        OnInventoryUpdated();
    }

    public override void _Draw()
    {
        if (InventoryData == null) return;

        // Sfondo
        DrawRect(new Rect2(0, 0, Size.X, Size.Y), new Color(0.2f, 0.2f, 0.2f, 1.0f));

        // Griglia
        var gridColor = new Color(0, 0, 0, 0.5f);
        for (int x = 0; x <= InventoryData.GridWidth; x++)
            DrawLine(new Vector2(x * TileSize, 0), new Vector2(x * TileSize, InventoryData.GridHeight * TileSize), gridColor);
        for (int y = 0; y <= InventoryData.GridHeight; y++)
            DrawLine(new Vector2(0, y * TileSize), new Vector2(InventoryData.GridWidth * TileSize, y * TileSize), gridColor);

        // Highlight
        if (_hoverRect.HasValue)
        {
            var highlightColor = _isHoverValid ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
            Rect2 pixelRect = new Rect2(
                _hoverRect.Value.Position.X * TileSize,
                _hoverRect.Value.Position.Y * TileSize,
                _hoverRect.Value.Size.X * TileSize,
                _hoverRect.Value.Size.Y * TileSize
            );
            DrawRect(pixelRect, highlightColor);
        }
    }

    private void OnInventoryUpdated()
    {
        if (!IsInstanceValid(this) || !IsInstanceValid(_itemsContainer)) return;

        foreach (var child in _itemsContainer.GetChildren())
            if (IsInstanceValid(child)) child.QueueFree();

        if (InventoryData == null) return;

        foreach (var itemInstance in InventoryData.Items)
        {
            var itemNode = ItemScene.Instantiate() as Control;
            _itemsContainer.AddChild(itemNode);

            // FIX ANCHORS
            itemNode.SetAnchorsPreset(LayoutPreset.TopLeft);
            itemNode.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
            itemNode.SizeFlagsVertical = SizeFlags.ShrinkBegin;

            float slotW = (itemInstance.Rotated ? itemInstance.SourceItem.Height : itemInstance.SourceItem.Width) * TileSize;
            float slotH = (itemInstance.Rotated ? itemInstance.SourceItem.Width : itemInstance.SourceItem.Height) * TileSize;
            Vector2 slotSize = new Vector2(slotW, slotH);

            itemNode.Position = new Vector2(itemInstance.GridX * TileSize, itemInstance.GridY * TileSize);
            itemNode.CustomMinimumSize = slotSize;
            itemNode.Size = slotSize;

            var iconNode = itemNode.GetNode<TextureRect>("Icon");
            if (iconNode != null)
            {
                iconNode.Texture = itemInstance.SourceItem.Icon;
                iconNode.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
                iconNode.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                iconNode.SetAnchorsPreset(LayoutPreset.TopLeft);

                float originalW = itemInstance.SourceItem.Width * TileSize;
                float originalH = itemInstance.SourceItem.Height * TileSize;
                Vector2 iconSize = new Vector2(originalW, originalH);

                iconNode.Size = iconSize;
                iconNode.PivotOffset = iconSize / 2;

                if (itemInstance.Rotated)
                {
                    iconNode.RotationDegrees = 90;
                    iconNode.Position = (slotSize / 2) - (iconSize / 2);
                }
                else
                {
                    iconNode.RotationDegrees = 0;
                    iconNode.Position = Vector2.Zero;
                    iconNode.Size = slotSize;
                }
            }
        }
    }

    // IsRecursivePlacement omesso per brevità (lascialo nel tuo codice)
    private bool IsRecursivePlacement(ItemData itemToDrop, InventoryData targetInventory)
    {
        if (itemToDrop.InternalInventory == null) return false;
        if (itemToDrop.InternalInventory == targetInventory) return true;
        var currentCheck = targetInventory;
        while (currentCheck != null)
        {
            if (currentCheck == itemToDrop.InternalInventory) return true;
            currentCheck = currentCheck.ParentInventory;
        }
        return false;
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        if (InventoryData == null) return default;
        int gridX = (int)(atPosition.X / TileSize);
        int gridY = (int)(atPosition.Y / TileSize);
        var item = InventoryData.GetItemAt(gridX, gridY);
        if (item == null) return default;

        // Setup Visuale Preview
        Vector2 size = new Vector2(item.SourceItem.Width * TileSize, item.SourceItem.Height * TileSize);

        var previewContainer = new Control();
        previewContainer.Size = size;
        previewContainer.PivotOffset = size / 2; // Perno centrale fondamentale per rotazione

        var previewIcon = new TextureRect();
        previewIcon.Texture = item.SourceItem.Icon;
        previewIcon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        previewIcon.Size = size;
        previewIcon.Modulate = new Color(1, 1, 1, 0.5f);
        previewIcon.Position = Vector2.Zero;

        previewContainer.AddChild(previewIcon);

        var wrapper = new Control();
        wrapper.AddChild(previewContainer);
        previewContainer.Position = -size / 2; // Centratura su mouse

        SetDragPreview(wrapper);

        bool currentRotation = item.Rotated;
        if (currentRotation)
        {
            previewContainer.RotationDegrees = 90;
        }

        return new DragDataInfo
        {
            ItemInstance = item,
            OriginalInventory = InventoryData,
            IsRotated = currentRotation,
            PreviewControl = previewContainer // <--- SALVIAMO QUI IL RIFERIMENTO
        };
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        var dragInfo = data.Obj as DragDataInfo;
        if (dragInfo == null) return false;

        ItemData sourceItemData = dragInfo.ItemInstance != null ? dragInfo.ItemInstance.SourceItem : dragInfo.SourceEquipmentSlot?.EquippedItem;
        if (sourceItemData == null) return false;

        int targetX = (int)(atPosition.X / TileSize);
        int targetY = (int)(atPosition.Y / TileSize);

        InventoryItemInstance itemToIgnore = null;
        if (dragInfo.OriginalInventory == InventoryData && dragInfo.ItemInstance != null)
        {
            itemToIgnore = dragInfo.ItemInstance;
        }

        bool recursiveError = IsRecursivePlacement(sourceItemData, InventoryData);
        bool spaceAvailable = InventoryData.CanPlaceItem(sourceItemData, targetX, targetY, itemToIgnore, dragInfo.IsRotated);

        // Calcolo dimensioni per rettangolo highlight
        int w = dragInfo.IsRotated ? sourceItemData.Height : sourceItemData.Width;
        int h = dragInfo.IsRotated ? sourceItemData.Width : sourceItemData.Height;

        _isHoverValid = spaceAvailable && !recursiveError;
        _hoverRect = new Rect2I(targetX, targetY, w, h);

        QueueRedraw();

        return true;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        ClearHighlight();
        var dragInfo = data.Obj as DragDataInfo;
        if (dragInfo == null) return;

        int targetX = (int)(atPosition.X / TileSize);
        int targetY = (int)(atPosition.Y / TileSize);

        ItemData sourceItemData = dragInfo.ItemInstance != null ? dragInfo.ItemInstance.SourceItem : dragInfo.SourceEquipmentSlot?.EquippedItem;
        if (sourceItemData == null) return;

        if (IsRecursivePlacement(sourceItemData, InventoryData))
        {
            GD.Print("Container Inception Error");
            return;
        }

        if (dragInfo.OriginalInventory != null)
            dragInfo.OriginalInventory.RemoveItem(dragInfo.ItemInstance);

        if (InventoryData.CanPlaceItem(sourceItemData, targetX, targetY, null, dragInfo.IsRotated))
        {
            InventoryData.AddItem(sourceItemData, targetX, targetY, dragInfo.IsRotated);

            if (dragInfo.SourceEquipmentSlot != null)
                InventoryManager.Instance.UnequipItem(dragInfo.SourceEquipmentSlot.SlotType);
        }
        else
        {
            if (dragInfo.OriginalInventory != null)
            {
                bool oldRot = dragInfo.ItemInstance.Rotated;
                dragInfo.OriginalInventory.AddItem(dragInfo.ItemInstance.SourceItem, dragInfo.ItemInstance.GridX, dragInfo.ItemInstance.GridY, oldRot);
            }
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.DoubleClick)
        {
            int gridX = (int)(mb.Position.X / TileSize);
            int gridY = (int)(mb.Position.Y / TileSize);
            var itemInstance = InventoryData.GetItemAt(gridX, gridY);
            if (itemInstance != null && itemInstance.SourceItem.InternalInventory != null)
            {
                InventoryManager.Instance.UIManager.OpenExternalContainer(
                    itemInstance.SourceItem.InternalInventory,
                    itemInstance.SourceItem.Name
                );
                GetViewport().SetInputAsHandled();
            }
        }
    }
}