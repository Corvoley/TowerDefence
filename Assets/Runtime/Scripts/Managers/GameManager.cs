using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [SerializeField] public Camera mainPlayerCamera;

    [SerializeField] public Transform[] spawnPoint;
    [SerializeField] public Transform[] playerSpawnPoint;
    [SerializeField] public Transform baseTransform;
    [SerializeField] public GameObject enemyPrefab;
    [SerializeField] public Button exitButton;

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

    public void AddPlayerToAlliesList(NetworkObject player)
    {

        if (!base.IsServerInitialized) return;
        if (!alliesNetworkObjectList.Contains(player))
        {
            alliesNetworkObjectList.Add(player);
            Debug.Log($"Player {player.Owner.ClientId} added. Total Players: {alliesNetworkObjectList.Count}");
        }     

    }
    public void RemovePlayerFromAlliesList(NetworkObject player)
    {

        if (!base.IsServerInitialized) return;
        if (alliesNetworkObjectList.Contains(player))
        {
            alliesNetworkObjectList.Remove(player);
            Debug.Log($"Player {player.Owner.ClientId} was removed. Total Players: {alliesNetworkObjectList.Count}");
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnResources(ResourceNodeOS resourceNodeOS, Vector3 spawnPos)
    {
        SpawnResourcesTask(resourceNodeOS, spawnPos);
    }

    private async void SpawnResourcesTask(ResourceNodeOS resourceNodeOS, Vector3 spawnPos)
    {
        int randomAmount = Random.Range(resourceNodeOS.minAmountToDrop, resourceNodeOS.maxAmountToDrop + 1);
        await Awaitable.EndOfFrameAsync();
        for (int i = 0; i < randomAmount; i++)
        {
            GameObject resource = Instantiate(resourceNodeOS.resourceToDrop.prefab, spawnPos, Quaternion.identity, null);
            Spawn(resource);
            
        }
    }
}
