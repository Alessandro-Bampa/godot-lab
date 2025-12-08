using Godot;
using Godot.Collections;

[GlobalClass]
[Tool]
public partial class GameSaveData : Resource
{
    [Export] public int PlayerLevel { get; set; } = 1;
    [Export] public float PlayerHealth { get; set; } = 100.0f;

    [Export] public Dictionary<ItemData, int> Inventory { get; set; } = new();
}

