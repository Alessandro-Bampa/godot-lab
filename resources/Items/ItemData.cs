using Godot;
using System;

[GlobalClass]
[Tool]
public partial class ItemData : Resource
{
    [Export] public string DisplayName { get; set; } = "Item";
    [Export] public Texture2D Icon { get; set; }
    [Export(PropertyHint.MultilineText)] public string Description { get; set; }
}
