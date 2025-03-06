using FishNet.CodeGenerating;
using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [SerializeField] public Camera mainPlayerCamera;

    [SerializeField] public Transform[] spawnPoint;
    [SerializeField] public Transform[] playerSpawnPoint;
    [SerializeField] public Transform baseTransform;
    [SerializeField] public GameObject enemyPrefab;    

    [SerializeField] public List<Transform> enemiesTransformList = new List<Transform>();

    [AllowMutableSyncType]
    [SerializeField]
    public SyncList<NetworkObject> alliesNetworkObjectList = new SyncList<NetworkObject>();


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
        obj.GetComponent<EnemyController>().defaultTarget = Instance.baseTransform;

    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnEnemy(GameObject obj)
    {
        Despawn(obj);
    }

    public void AddPlayerToList(NetworkObject player)
    {

        if (!base.IsServerInitialized) return;
        if (!alliesNetworkObjectList.Contains(player))
        {
            alliesNetworkObjectList.Add(player);
            Debug.Log($"Player {player.Owner.ClientId} added. Total Players: {alliesNetworkObjectList.Count}");
        }
      

    }    
}
