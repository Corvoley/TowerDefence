using UnityEngine;
using UnityEngine.AI;

public class WallController : MonoBehaviour, IConstructable, IEnemyTarget
{   

    [SerializeField] private PlaceableSO placeableToConstruct;

    [SerializeField] private Transform modelTransform;

    [SerializeField] private Collider mainCollider;

    [SerializeField] private Material transparentMaterial;

    [SerializeField] private NavMeshObstacle navMeshObstacle;
    [SerializeField] private bool isActive = false;


    public PlaceableSO PlaceableToConstruct => placeableToConstruct;

    public Transform ModelTransform => modelTransform;

    public Collider MainCollider => mainCollider;

    public Material TransparentMaterial => transparentMaterial;

    public IEnemyTarget.TargetType Type => IEnemyTarget.TargetType.Wall;


    public bool IsActive => isActive;

    public void OnConstructionFinished()
    {
        navMeshObstacle.enabled = true;
        isActive = true;
    }
}
