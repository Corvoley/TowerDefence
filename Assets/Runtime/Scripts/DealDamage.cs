using UnityEngine;

public class DealDamage : MonoBehaviour
{
    [SerializeField] private bool CanFriendlyFire;
    [SerializeField] private float damage;
    [SerializeField] private string nameTag;

    private void Start()
    {
        nameTag = gameObject.transform.root.tag;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (CanFriendlyFire)
        {
            HealthController healthController = other.GetComponent<HealthController>();
            if (healthController != null)
                healthController.DealDamage(damage);
        }
        else
        {
            if (nameTag != other.transform.root.tag)
            {
                HealthController healthController = other.GetComponent<HealthController>();
                if (healthController != null)
                    healthController.DealDamage(damage);
            }
        }

        
    }
}
