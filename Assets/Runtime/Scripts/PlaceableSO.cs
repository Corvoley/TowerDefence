using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "pl_", menuName = "Placeables/Placeable", order = 0)]
public class PlaceableSO : ScriptableObject
{
    [Header("Placement")]
    public string objName;
    public GameObject objPrefab;
    public float placementRadius;

    public List<ItemAmount> constructionResourceList;
}
