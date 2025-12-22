using Godot;


[GlobalClass]
public partial class InventoryData : Resource
{
    [Export] public Godot.Collections.Array<InventoryItemInstance> Items { get; set; } = new();
    [Export] public int GridWidth { get; set; } = 10;
    [Export] public int GridHeight { get; set; } = 10;

    [Signal] public delegate void InventoryUpdatedEventHandler();

    public InventoryData ParentInventory { get; set; } = null;

    // Ritorna l'oggetto in una specifica coordinata, se presente
    public InventoryItemInstance GetItemAt(int x, int y)
    {
        foreach (var item in Items)
        {
            Rect2I itemRect = new Rect2I(item.GridX, item.GridY, item.SourceItem.Width, item.SourceItem.Height);
            if (itemRect.HasPoint(new Vector2I(x, y)))
            {
                return item;
            }
        }
        return null;
    }

    // Controlla se c'è spazio per un oggetto
    public bool CanPlaceItem(ItemData item, int x, int y, InventoryItemInstance ignoreItem = null, bool rotated = false)
    {
        // 1. Calcola le dimensioni del NUOVO oggetto che stiamo provando a mettere
        int w = rotated ? item.Height : item.Width;
        int h = rotated ? item.Width : item.Height;

        // Controllo bordi
        if (x < 0 || y < 0 || x + w > GridWidth || y + h > GridHeight)
            return false;

        Rect2I newRect = new Rect2I(x, y, w, h);

        // 2. Controllo collisioni con gli oggetti ESISTENTI
        foreach (var existingItem in Items)
        {
            if (existingItem == ignoreItem) continue;

            // --- QUI STA LA MAGIA ---
            // Usiamo le dimensioni dinamiche dell'istanza esistente (che potrebbe essere ruotata)
            int existingW = existingItem.Rotated ? existingItem.SourceItem.Height : existingItem.SourceItem.Width;
            int existingH = existingItem.Rotated ? existingItem.SourceItem.Width : existingItem.SourceItem.Height;

            Rect2I existingRect = new Rect2I(existingItem.GridX, existingItem.GridY, existingW, existingH);

            if (newRect.Intersects(existingRect))
                return false;
        }
        return true;
    }

    public void AddItem(ItemData item, int x, int y, bool rotated = false)
    {
        var newItem = new InventoryItemInstance(item, x, y, rotated);
        Items.Add(newItem);

        if (item.InternalInventory != null)
        {
            item.InternalInventory.ParentInventory = this;
        }

        EmitSignal(SignalName.InventoryUpdated);
    }

    public void AddItem(InventoryItemInstance newItem)
    {
        Items.Add(newItem);
        if (newItem.SourceItem.InternalInventory != null)
        {
            newItem.SourceItem.InternalInventory.ParentInventory = this;
        }
        EmitSignal(SignalName.InventoryUpdated);
    }

    public void RemoveItem(InventoryItemInstance item)
    {
        if (Items.Contains(item))
        {
            Items.Remove(item);
            if (item.SourceItem.InternalInventory != null)
            {
                item.SourceItem.InternalInventory.ParentInventory = null;
            }
            EmitSignal(SignalName.InventoryUpdated);
        }
    }
}
