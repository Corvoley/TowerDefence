using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

public class PlacementController : MonoBehaviour
{
    [SerializeField] private PlaceableSO objToPlace;



    [SerializeField] private PlaceableSO objBeingPlaced;
    [SerializeField] private Image placementImage;

    [SerializeField] private Color validColor;
    [SerializeField] private Color invalidColor;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask invalidMask;
    [SerializeField] private float radius;

    [SerializeField] private bool drawGizmos;


    private Camera maincCamera;

    private void Awake()
    {
        maincCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            SetObjToPlace(objToPlace);
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            PlaceObject(objBeingPlaced);
        }
        if (placementImage.gameObject.activeInHierarchy)
        {
            (bool success, Vector3 position) = UtilsClass.GetMouseWorldPosition(groundMask);
            if (success)
            {
                placementImage.canvas.transform.position = position;
                if (IsPositionValid(position, placementImage.rectTransform.rect.width / 2))
                {
                    placementImage.color = validColor;
                }
                else
                {
                    placementImage.color = invalidColor;
                }
            }
        }
    }

 
    private void PlaceObject(PlaceableSO placeableSO)
    {
        if (placeableSO == null) return;
        (bool success, Vector3 position) = UtilsClass.GetMouseWorldPosition(groundMask);
        if (success && IsPositionValid(position, placeableSO.placementRadius))
        {           
            GameManager.Instance.SpawnPlaceable(placeableSO, position);
            SetObjToPlace(null);
        }
    }

    public void SetObjToPlace(PlaceableSO placeableSO)
    {
        objBeingPlaced = placeableSO;
        if (objBeingPlaced != null)
        {
            placementImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, placeableSO.placementRadius * 2);
            placementImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, placeableSO.placementRadius * 2);
            placementImage.gameObject.SetActive(true);
        } 
        else 
        { 
            placementImage.gameObject.SetActive(false);
        }

    }
    private bool IsPositionValid(Vector3 pos, float radius)
    {
        return !Physics.CheckSphere(pos, radius, invalidMask);
    }

    private void OnDrawGizmos()
    {
        if (drawGizmos)
        {
            var pos = UtilsClass.GetMouseWorldPosition(groundMask);
            if (pos.success && objBeingPlaced != null)
            {
               
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(pos.position, objBeingPlaced.placementRadius);
            }

        }

    }

}
