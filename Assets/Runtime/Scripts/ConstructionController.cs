using System.Linq;
using UnityEngine;

public class ConstructionController : MonoBehaviour, IInteractable
{
    [SerializeField] private PlaceableSO objToPlace;
    [SerializeField] private Inventory inventory;

    public void SetupConstruction(PlaceableSO objToPlace)
    {
        this.objToPlace = objToPlace;
        inventory.inventoryObjects = objToPlace.constructionResourceList;
    }

    private void AddResourceToSpot(PlayerController playerController)
    {
        if (inventory.inventoryObjects.Count <= 0)
        {
            return;
        }
        var item = inventory.inventoryObjects[0];

        InventoryUtils.RemoveItemFromInventory(playerController.gameObject.GetComponentInChildren<Inventory>(), item.itemSO, (bool result) =>
        {
            if (result)
            {
                InventoryUtils.RemoveItemFromInventory(inventory, item.itemSO, (result) => { });
                if (IsMaterialsComplete())
                {
                    FinishContruction();
                }
                
            }

        });
    }

    private bool IsMaterialsComplete()
    {
        return inventory.inventoryObjects.Count <= 0;
    }

    private void FinishContruction()
    {
        GameManager.Instance.SpawnPlaceable(objToPlace, transform.position, transform.rotation);
        GameManager.Instance.Despawn(gameObject);
    }

    public void OnInteract(PlayerController playerController)
    {
        AddResourceToSpot(playerController);
    }
}
