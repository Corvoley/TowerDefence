using System;
using UnityEngine;

public static class InventoryUtils
{
    public static void AddItemToInventory(Inventory inventory, ItemSO newItemSO)
    {
        foreach (ItemAmount item in inventory.inventoryObjects)
        {
            if (item.itemSO.itemName == newItemSO.itemName)
            {
                item.amount++;
                return;
            }
        }
        inventory.inventoryObjects.Add(new ItemAmount { itemSO = newItemSO, amount = 1 });
    }
    public static void RemoveItemFromInventory(Inventory inventory, ItemSO newItemSO, Action<bool> result)
    {
        foreach (ItemAmount item in inventory.inventoryObjects)
        {
            if (item.itemSO.itemName == newItemSO.itemName)
            {
                item.amount--;
                if (item.amount <= 0)
                {
                    inventory.inventoryObjects.Remove(item);
                }

                //Debug.Log($"Resource removed");
                result(true);
                inventory.OnInventoryChanged?.Invoke();
                return;
            }

            //Debug.Log("Resource not removed");
            result(false);
        }

    }
}
