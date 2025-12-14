using Godot;

[GlobalClass]
public partial class InventoryItemInstance : Resource
{
    [Export] public ItemData SourceItem { get; set; }
    [Export] public int GridX { get; set; }
    [Export] public int GridY { get; set; }

    // Costruttore vuoto richiesto da Godot
    public InventoryItemInstance() { }

    public InventoryItemInstance(ItemData item, int x, int y)
    {
        SourceItem = item;
        GridX = x;
        GridY = y;
    }
}
