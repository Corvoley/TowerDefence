using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {        
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Attack!!!!");
        }
    }
}
