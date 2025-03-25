using FishNet.Object;
using UnityEngine;

public class ProjectileController : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed;
    [SerializeField] private LayerMask layerToHit;

    public void SetupProjectile(Vector3 targetPos)
    {

        var dir = targetPos - transform.position;
        transform.forward = dir;


        rb.AddForce(transform.forward * speed, ForceMode.Impulse);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & layerToHit.value) != 0)
        {
            Destroy(gameObject, 0.1f);
        }
    }
}
