using FishNet.Object;
using System.Collections;
using System.Threading;
using UnityEngine;

public class TurretController : NetworkBehaviour
{
    [SerializeField] private GameObject projectilePrefab;

    [SerializeField] private Transform nearestTarget;
    [SerializeField] private Transform turretHead;
    [SerializeField] private float range;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private bool canShoot;
    [SerializeField] private bool canRotate;
    [SerializeField] private float shootDelay;

    private float shootTimer;
    private Vector3 nearestTargetPos;

    private HealthController healthController;

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
        if (nearestTarget != null && canRotate)
        {
            UtilsClass.RotateToTarget(turretHead, nearestTarget, rotationSpeed, 0, false);
        }


    }

    private void Shoot()
    {
        if (!canShoot) { return; }
        if (nearestTarget == null) { return; }
        if (Time.time >= shootTimer)
        {
            shootTimer = Time.time + shootDelay;
            nearestTargetPos = nearestTarget.position;
            ShootRPC();
        }

    }
    private IEnumerator FindNearestTargetWithinRangeWithDelay(float delay)
    {
        while (true)
        {
            Collider[] targetsInViewRadius = Physics.OverlapSphere(turretHead.position, range, targetMask);

            if (nearestTarget != null)
            {
                if (Vector3.Distance(nearestTarget.position, turretHead.position) > range)
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
                        float distance = Vector3.Distance(turretHead.position, targetsInViewRadius[i].transform.position);
                        if (distance < Vector3.Distance(turretHead.position, closest.position))
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
        GameObject projectile = Instantiate(projectilePrefab, turretHead.position, Quaternion.identity, null);

        Spawn(projectile);
        projectile.GetComponent<ProjectileController>().SetupProjectile(nearestTargetPos);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, range);


    }
}
