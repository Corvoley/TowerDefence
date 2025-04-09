using FishNet.Object;
using System.Collections;
using System.Threading;
using UnityEngine;

public class TurretController : NetworkBehaviour, IConstructable
{
    [SerializeField] private Transform nearestTarget;
    [SerializeField] private Transform turretHead;
    [SerializeField] private Canvas placementCanvas;

    [SerializeField] private TurretSO turretSO;
    [SerializeField] private Material transparentMat;
       
    [SerializeField] private bool canShoot;

    private float shootTimer;
    private Vector3 nearestTargetPos;

    private HealthController healthController;

    public PlaceableSO PlaceableToConstruct  => turretSO;
    public Transform ModelTransform => gameObject.transform.Find("Model");
    public Material TransparentMaterial => transparentMat;

    private void Awake()
    {
        healthController = GetComponentInChildren<HealthController>();
    }

    private void OnDeath(GameObject obj)
    {
        Destroy(gameObject);
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(FindNearestTargetWithinRangeWithDelay(0.2f));
        
    }

    private void Update()
    {
        RotateToTarget();
        Shoot();
    }
    private void RotateToTarget()
    {
        if (nearestTarget != null && turretSO.rotationSpeed > 0)
        {
            UtilsClass.RotateToTarget(turretHead, nearestTarget, turretSO.rotationSpeed, 0, false);
        }


    }

    private void Shoot()
    {
        if (!canShoot) { return; }
        if (nearestTarget == null) { return; }
        if (Time.time >= shootTimer)
        {
            shootTimer = Time.time + turretSO.shootDelay;
            nearestTargetPos = nearestTarget.position;
            ShootRPC();
        }

    }
    private IEnumerator FindNearestTargetWithinRangeWithDelay(float delay)
    {
        while (true)
        {
            Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, turretSO.attackRadius, turretSO.targetLayer);

            if (nearestTarget != null)
            {
                if (Vector3.Distance(nearestTarget.position, transform.position) > turretSO.attackRadius)
                {
                    nearestTarget = null;
                }
            }
            else
            {
                Transform closest = null;
                if (nearestTarget == null && targetsInViewRadius.Length > 0)
                {
                    closest = targetsInViewRadius[0].transform;

                    for (int i = 0; i < targetsInViewRadius.Length; i++)
                    {
                        float distance = Vector3.Distance(transform.position, targetsInViewRadius[i].transform.position);
                        if (distance < Vector3.Distance(transform.position, closest.position))
                        {
                            closest = targetsInViewRadius[i].transform;
                        }
                    }
                }
                nearestTarget = closest;
            }
            yield return new WaitForSeconds(delay);
        }

    }


    [ServerRpc(RequireOwnership = false)]
    private void ShootRPC()
    {
        Debug.Log("Shooting");
        GameObject projectile = Instantiate(turretSO.projectilePrefab, turretHead.position, Quaternion.identity, null);

        Spawn(projectile);
        projectile.GetComponent<ProjectileController>().SetupProjectile(nearestTargetPos);
    }


    private void OnDrawGizmosSelected()
    {
        if (turretSO != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, turretSO.attackRadius);

        }


    }

    public void OnConstructionFinished()
    {

        Debug.Log("Tower Fineshed");
    }
}
