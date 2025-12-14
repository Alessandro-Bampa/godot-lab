using Godot;


[GlobalClass]
public partial class InventoryData : Resource
{
    [Export] public Godot.Collections.Array<InventoryItemInstance> Items { get; set; } = new();
    [Export] public int GridWidth { get; set; } = 10;
    [Export] public int GridHeight { get; set; } = 10;

    [Signal] public delegate void InventoryUpdatedEventHandler();

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
    public bool CanPlaceItem(ItemData item, int x, int y, InventoryItemInstance ignoreItem = null)
    {
        // 1. Controllo bordi
        if (x < 0 || y < 0 || x + item.Width > GridWidth || y + item.Height > GridHeight)
            return false;

        // 2. Rettangolo del nuovo oggetto
        Rect2I newRect = new Rect2I(x, y, item.Width, item.Height);

        // 3. Controllo collisioni con altri oggetti
        foreach (var existingItem in Items)
        {
            if (existingItem == ignoreItem) continue; // Ignora se stesso (utile per lo spostamento)

            Rect2I existingRect = new Rect2I(existingItem.GridX, existingItem.GridY, existingItem.SourceItem.Width, existingItem.SourceItem.Height);

            if (newRect.Intersects(existingRect))
                return false;
        }
        return true;
    }

    public void AddItem(ItemData item, int x, int y)
    {
        var newItem = new InventoryItemInstance(item, x, y);
        Items.Add(newItem);
        EmitSignal(SignalName.InventoryUpdated);
    }

    public void RemoveItem(InventoryItemInstance item)
    {
        if (Items.Contains(item))
        {
            Items.Remove(item);
            EmitSignal(SignalName.InventoryUpdated);
        }
    }
}
