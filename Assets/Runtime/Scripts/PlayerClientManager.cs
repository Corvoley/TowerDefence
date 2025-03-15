using FishNet;
using FishNet.CodeGenerating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Steamworks;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerClientManager : NetworkBehaviour
{
    //[SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerSpawnedRef;
    [SerializeField] private GameObject bootstrapNetworkPrefab;
    [SerializeField] private GameObject bootstrapNetworkRef;



    [AllowMutableSyncType]
    [SerializeField]
    private SyncVar<string> username = new SyncVar<string>();
    private void Awake()
    {

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        BootstrapManager.OnHostDisconnected += BootstrapManager_OnHostDisconnected;

    }

    private async void BootstrapManager_OnHostDisconnected()
    {
        await LeaveMatch();
    }

    //only works when called directly on the scene
    public override void OnStartClient()
    {
        base.OnStartClient();


        if (IsOwner)
        {
            if (IsServerInitialized)
            {
                SpawnBootstrapNetwork();
            }

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

                SetUsername(SteamFriends.GetPersonaName().ToString());
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
        GameObject playerObj = Instantiate(playerPrefab, transform.position, Quaternion.identity);
        GameObject modelObj = playerObj.transform.GetChild(0).gameObject;
        GameObject worldCanvasObj = playerObj.transform.GetChild(1).gameObject;
        GameObject mainCanvasObj = playerObj.transform.GetChild(2).gameObject;



        modelObj.SetActive(false);
        worldCanvasObj.SetActive(false);
        mainCanvasObj.SetActive(false);

        Spawn(playerObj, conn);
        SetSpawnedPlayer(playerObj, clientScript);
        SetUsernameOnPlayer(this, username.Value);
        SetClientRefOnPlayer(this);

    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnPlayer()
    {
        Despawn(playerSpawnedRef);
    }

    [ObserversRpc]
    public void SetSpawnedPlayer(GameObject playerObj, PlayerClientManager clientScript)
    {
        clientScript.playerSpawnedRef = playerObj;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBootstrapNetwork()
    {
        var bootstrap = Instantiate(bootstrapNetworkPrefab);
        Spawn(bootstrap);
        SetBootstrapNetworkRefForPlayers(bootstrap);
    }
    [ServerRpc(RequireOwnership = false)]
    public void DespawnBootstrapNetwork()
    {
        Despawn(bootstrapNetworkRef);
    }
    [ObserversRpc(BufferLast = true)]
    public void SetBootstrapNetworkRefForPlayers(GameObject boostrapRef)
    {
        bootstrapNetworkRef = boostrapRef;
    }

    [ObserversRpc]
    public void SetUsernameOnPlayer(PlayerClientManager script, string username)
    {
        script.playerSpawnedRef.GetComponent<PlayerController>().SetUsername(username);
    }
    [ObserversRpc]
    public void SetClientRefOnPlayer(PlayerClientManager script)
    {
        script.playerSpawnedRef.GetComponent<PlayerController>().playerClientManagerRef = script;
    }

    public async Task LeaveMatch()
    {
        if (!IsOwner) { return; };
        DespawnPlayer();
        await Awaitable.WaitForSecondsAsync(0.5f);
        BootstrapManager.LeaveLobby();
        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainMenuScene", LoadSceneMode.Additive);
        await Awaitable.WaitForSecondsAsync(0.5f);
        if (UnityEngine.SceneManagement.SceneManager.GetSceneByName("GameScene").IsValid())
        {
            await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("GameScene");
        }

    }
    public override void OnStopServer()
    {
        base.OnStopServer();
        if (IsServerInitialized)
        {
            DespawnBootstrapNetwork();
        }

    }
}
