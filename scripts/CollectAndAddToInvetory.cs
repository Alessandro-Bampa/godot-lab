using Godot;
using System;

public partial class CollectAndAddToInvetory : Collectable
{
    [Export] public ItemData ItemReference;
    [Export] public int Quantity = 1;

    public override void OnCollect()
    {
        if (ItemReference == null)
        {
            GD.PushError($"Attenzione: {Name} non ha una ItemData assegnata!");
            return;
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(ItemReference, Quantity);
        }
    }

}
