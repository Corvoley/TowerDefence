using FishNet.Object;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    [SerializeField] public Camera mainPlayerCamera;

    [SerializeField] public Transform[] spawnPoint;
    [SerializeField] public Transform playerSpawnPoint;
    [SerializeField] public Transform baseTransform;
    [SerializeField] public GameObject enemyPrefab;


    [SerializeField] public List<Transform> enemiesTransformList = new List<Transform>();
    [SerializeField] public List<Transform> alliesTransformList = new List<Transform>();    


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
        var randomPoint = Random.Range(0, spawnPoint.Length);
        GameObject obj = Instantiate(enemyPrefab, spawnPoint[randomPoint].position, Quaternion.identity);
        Spawn(obj);
        obj.GetComponent<EnemyController>().defaultTarget = instance.baseTransform;
        
    }
    [ServerRpc(RequireOwnership = false)]

    public void DespawnEnemy(GameObject obj)
    {
        Despawn(obj);
    }
}
