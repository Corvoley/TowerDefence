using FishNet.Object;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    public List<InventoryObject> inventoryObjects = new List<InventoryObject>();

    [Serializable]
    public class InventoryObject
    {
        public ItemSO itemSO;
        public int amount;
    }

}
