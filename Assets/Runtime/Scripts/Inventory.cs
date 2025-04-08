using FishNet.Object;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    public Action OnInventoryChanged;
    public List<ItemAmount> inventoryObjects = new List<ItemAmount>();

    

}
