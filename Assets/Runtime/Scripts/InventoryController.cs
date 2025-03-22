using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Inventory))]
public class InventoryController : NetworkBehaviour
{

    [SerializeField] private GameObject itemTemplate;
    [SerializeField] private Transform itemHolderTransform;

    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Inventory inventory;
    [SerializeField] private Transform pickupPositionTransform;
    [SerializeField] private Transform dropPositionTransform;

    [Header("Setttings")]
    [SerializeField] private float pickupRadius;
    [SerializeField] private LayerMask pickupLayer;
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            this.enabled = false;
            return;
        }
        else
        {
            inventory = GetComponent<Inventory>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            PickUpItem();

        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void PickUpItem()
    {
        var itemColliders = Physics.OverlapSphere(pickupPositionTransform.position, pickupRadius, pickupLayer);

        if (itemColliders.Length > 0)
        {

            ItemInfoHolder item;
            foreach (var itemCollider in itemColliders)
            {
                if (itemCollider.TryGetComponent(out item))
                {
                    AddItemToInventory(item.itemSO);
                    DespawnItemObjFromWorld(item.gameObject);
                }
            }
            UpdateInvUI();
        }
    }

    private void DropItem(ItemSO itemSO)
    {
        foreach (Inventory.InventoryObject invObj in inventory.inventoryObjects)
        {
            if (invObj.itemSO != itemSO)
            {
                continue;
            }
            if (invObj.amount > 1)
            {
                invObj.amount--;
                SpawnItemObjIntoWorld(invObj.itemSO.prefab);
                return;
            }
            if (invObj.amount < 1)
            {
                inventory.inventoryObjects.Remove(invObj);
                SpawnItemObjIntoWorld(invObj.itemSO.prefab);
                return;
            }
        }
        UpdateInvUI();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnItemObjIntoWorld(GameObject objToSpawn)
    {
        GameObject worldObj = Instantiate(objToSpawn, dropPositionTransform.position, Quaternion.identity, null);
        ServerManager.Spawn(worldObj);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnItemObjFromWorld(GameObject objToDespawn)
    {
        ServerManager.Despawn(objToDespawn, DespawnType.Destroy);
    }
    private void AddItemToInventory(ItemSO newItemSO)
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
    private void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        UpdateInvUI();
    }

    private void UpdateInvUI()
    {
        foreach (Transform child in itemHolderTransform)
        {
            Destroy(child.gameObject);
        }

        foreach (Inventory.InventoryObject invObj in inventory.inventoryObjects)
        {
            GameObject obj = Instantiate(itemTemplate, itemHolderTransform);
            obj.transform.Find("name").GetComponent<TextMeshProUGUI>().text = invObj.itemSO.itemName.ToString();
            obj.transform.Find("amount").GetComponent<TextMeshProUGUI>().text = invObj.amount.ToString();
            obj.transform.GetComponent<Button>().onClick.AddListener(() => { DropItem(invObj.itemSO);});
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pickupPositionTransform.position, pickupRadius);

    }
}
