using FishNet.Object;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class ConstructionController : NetworkBehaviour, IInteractable
{
    [SerializeField] private Inventory inventory;

    [SerializeField] private IConstructable iConstructable;

    [SerializeField] private List<MeshRenderer> meshRendererList = new List<MeshRenderer>(); 
    [SerializeField] private Material originalMat;

    public override void OnStartClient()
    {
        base.OnStartClient();
        inventory = GetComponent<Inventory>();
        iConstructable = GetComponent<IConstructable>();
        SetupConstruction(iConstructable.PlaceableToConstruct);
    }

    public void SetupConstruction(PlaceableSO objToPlace)
    {
        inventory.inventoryObjects = objToPlace.constructionResourceList.Select(item => new ItemAmount{ itemSO = item.itemSO, amount = item.amount}).ToList(); ;
        meshRendererList = iConstructable.ModelTransform.GetComponentsInChildren<MeshRenderer>().ToList();
        originalMat = meshRendererList[0].material;
        iConstructable.MainCollider.isTrigger = true;

        foreach (MeshRenderer meshRenderer in meshRendererList)
        {
            meshRenderer.sharedMaterial = iConstructable.TransparentMaterial;
        }
    }
    [ServerRpc(RequireOwnership = false)]
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

    [ServerRpc(RequireOwnership = false)]
    private void FinishContruction()
    {
        iConstructable.OnConstructionFinished();
        iConstructable.MainCollider.isTrigger = false;
        foreach (MeshRenderer meshRenderer in meshRendererList)
        {
            meshRenderer.sharedMaterial = originalMat;
        }

        Destroy(this);
        Destroy(inventory);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnInteract(PlayerController playerController)
    {
        AddResourceToSpot(playerController);
    }
}
