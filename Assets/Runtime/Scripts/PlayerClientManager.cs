using FishNet;
using FishNet.Component.Spawning;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System;
using UnityEngine;

public class PlayerClientManager : NetworkBehaviour
{
    //[SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
    {
        if (arg0.name == "GameScene")
        {
            if (IsOwner)
            {
                InstanceFinder.SceneManager.AddConnectionToScene(LocalConnection, arg0);                
                SpawnPlayer(LocalConnection);

            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayer(NetworkConnection conn)
    {
        GameObject obj = Instantiate(playerPrefab, transform.position, Quaternion.identity);
        Spawn(obj, conn);
    }


}
