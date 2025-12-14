using Godot;
using System;

[GlobalClass]
public partial class ItemData : Resource
{
    [Export] public string Name { get; set; } = "Item";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
    [Export] public Texture2D Icon { get; set; }

    // Dimensioni nella griglia (es. 2x5 per un fucile)
    [Export] public int Width { get; set; } = 1;
    [Export] public int Height { get; set; } = 1;

    // Se l'oggetto è un contenitore (Zaino, Gilet), assegniamo qui una risorsa InventoryData.
    // Se è null, l'oggetto non è un contenitore.
    [Export] public InventoryData InternalInventory { get; set; }
}