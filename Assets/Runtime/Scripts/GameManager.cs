using FishNet.Object;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    [SerializeField] public Camera mainPlayerCamera;

    [SerializeField] public Transform spawnPoint;
    [SerializeField] public GameObject enemyPrefab;


    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnEnemy();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SpawnEnemy()
    {
        GameObject obj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        Spawn(obj);
    }
}
