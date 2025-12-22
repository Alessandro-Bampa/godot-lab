using Godot;

[GlobalClass]
public partial class InventoryItemInstance : Resource
{
    [Export] public ItemData SourceItem { get; set; }
    [Export] public int GridX { get; set; }
    [Export] public int GridY { get; set; }
    [Export] public bool Rotated { get; set; } = false;

    // Costruttore vuoto richiesto da Godot
    public InventoryItemInstance() { }

    public InventoryItemInstance(ItemData item, int x, int y, bool rotated = false)
    {
        SourceItem = item;
        GridX = x;
        GridY = y;
        Rotated = rotated;
    }

    public int GetWidth() => Rotated ? SourceItem.Height : SourceItem.Width;
    public int GetHeight() => Rotated ? SourceItem.Width : SourceItem.Height;
}
