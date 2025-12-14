using Godot;
using Godot.Collections;

[GlobalClass]
[Tool]
public partial class GameSaveData : Resource
{
    [Export] public int PlayerLevel { get; set; } = 1;
    [Export] public float PlayerHealth { get; set; } = 100.0f;

    [Export] public Dictionary<string, ItemData> Equipment { get; set; } = new();

    [Export] public InventoryData Pockets { get; set; }

    public GameSaveData()
    {
        Equipment = new Dictionary<string, ItemData>();

        // Inizializza le tasche vuote (es. 5x2 slot)
        Pockets = new InventoryData();
        Pockets.GridWidth = 5;
        Pockets.GridHeight = 2;
    }
}

