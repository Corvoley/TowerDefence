using FishNet;
using FishNet.CodeGenerating;
using FishNet.Component.Spawning;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Steamworks;
using System;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class PlayerClientManager : NetworkBehaviour
{
    //[SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerSpawnedRef;
    [AllowMutableSyncType]
    [SerializeField]
    private SyncVar<string> username = new SyncVar<string>();
    private void Awake()
    {
      
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
       
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            SetUsername(SteamFriends.GetPersonaName().ToString());
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "GameScene")
            {
                //InstanceFinder.SceneManager.AddConnectionToScene(LocalConnection, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                SpawnPlayer(LocalConnection, this);
                
            }
        }
        
    }
    private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode sceneMode)
    {
        if (IsOwner)
        {
            if (scene.name == "GameScene")
            {
                //InstanceFinder.SceneManager.AddConnectionToScene(LocalConnection, arg0);                
                SpawnPlayer(LocalConnection, this);                
            }
        }
       
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetUsername(string name)
    {
        username.Value = name;        
        //usernameText.text = username;
        Debug.Log("Nome de usuario setado: " + username.Value);

    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayer(NetworkConnection conn, PlayerClientManager clientScript)
    {
        GameObject obj = Instantiate(playerPrefab, transform.position, Quaternion.identity);        
        Spawn(obj, conn);
        SetSpawnedPlayer(obj, clientScript);
        SetUsernameOnPlayer(this, username.Value);  
    }
    [ObserversRpc]
    public void SetSpawnedPlayer(GameObject playerObj, PlayerClientManager clientScript)
    {
        clientScript.playerSpawnedRef = playerObj;
    }
    [ObserversRpc]
    public void SetUsernameOnPlayer(PlayerClientManager script,string username)
    {
        script.playerSpawnedRef.GetComponent<PlayerController>().SetUsername(username);
    }
}
