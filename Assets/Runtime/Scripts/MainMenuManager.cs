using Steamworks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private static MainMenuManager instance;


    [SerializeField] private GameObject menuScreen;
    [SerializeField] private GameObject lobbyScreen;

    [SerializeField] private TMP_InputField lobbyInput;
    [SerializeField] private TextMeshProUGUI lobbyTitle;
    [SerializeField] private TextMeshProUGUI lobbyIDText;

    [SerializeField] private Button startGameButton;


    

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        OpenMainScreen();
    }
    public void CreateLobby()
    {
        BootstrapManager.CreateLobby();
        
    }
    public void OpenMainScreen()
    {
        ClosseAllScreens();
        menuScreen.SetActive(true);
    }
    public void OpenLobbyScreen()
    {
        ClosseAllScreens();
        lobbyScreen.SetActive(true);
    }

    public void JoinLobby()
    {
        CSteamID steamID = new CSteamID(Convert.ToUInt64(lobbyInput.text));
        BootstrapManager.JoinByID(steamID);
    }
    public void LeaveLobby()
    {
        BootstrapManager.LeaveLobby();
        OpenMainScreen();  
    }

    public static void LobbyEntered(string lobbyName, bool isHost)
    {
        instance.lobbyTitle.text = lobbyName;
        instance.startGameButton.gameObject.SetActive(isHost);
        instance.lobbyIDText.text = BootstrapManager.CurrentLobbyID.ToString();
        instance.OpenLobbyScreen();
    }
    public void CopyText()
    {
        GUIUtility.systemCopyBuffer = lobbyIDText.text;
    }
    private void ClosseAllScreens()
    {
        menuScreen.SetActive(false);
        lobbyScreen.SetActive(false);
    }

    public void StartGame()
    {
        string[] scenesToClose = new string[] { "MainMenuScene" };
        BootstrapNetworkManager.ChangeNetworkScene("GameScene", scenesToClose);
    }

}
