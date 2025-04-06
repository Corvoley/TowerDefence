
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "t_", menuName = "Placeables/Turret", order = 0)]
public class TurretSO : PlaceableSO
{     

    [Header("Stats")]
    public GameObject projectilePrefab;
    public LayerMask targetLayer;
    public float attackRadius;
    public float rotationSpeed;
    public float shootDelay;

    public List<ItemAmount> constructionResourceList;

}
