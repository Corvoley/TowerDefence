using UnityEngine;

public static class InventoryUtils
{
    public static void AddItemToInventory(Inventory inventory, ItemSO newItemSO)
    {
        foreach (Inventory.InventoryObject invObj in inventory.inventoryObjects)
        {
            if (invObj.itemSO == newItemSO)
            {
                invObj.amount++;
                return;
            }
        }
        inventory.inventoryObjects.Add(new Inventory.InventoryObject() { itemSO = newItemSO, amount = 1 });
    }
    public static void RemoveItemFromInventory(Inventory inventory, ItemSO newItemSO)
    {
        foreach (Inventory.InventoryObject invObj in inventory.inventoryObjects)
        {
            if (invObj.itemSO == newItemSO)
            {
                invObj.amount--;
                if (invObj.amount <= 0)
                {
                    inventory.inventoryObjects.Remove(invObj);
                }
                return;
            }
        }

    }
}
