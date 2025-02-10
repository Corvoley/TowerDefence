using FishNet.Object;
using UnityEngine;

public class ClientManager : NetworkBehaviour
{
    private void Awake()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
    {
        
    }

}
