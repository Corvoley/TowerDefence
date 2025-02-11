using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] public Camera mainPlayerCamera;

    [SerializeField] public Transform spawnPoint;

    
    private void Awake()
    {
        instance = this;
    }
}
