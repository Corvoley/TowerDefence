using UnityEngine;


[CreateAssetMenu(fileName = "rn_", menuName = "Item/ResourceNode", order = 0)]
public class ResourceNodeOS : ScriptableObject
{
    public string nodeName;
    public GameObject nodePrefab;

    public ResourceSO resourceToDrop;
    public int minAmountToDrop;
    public int maxAmountToDrop;
}
