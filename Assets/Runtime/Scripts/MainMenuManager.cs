using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;


    [SerializeField] private GameObject menuScreen;
    [SerializeField] private GameObject lobbyScreen;

    [SerializeField] private GameObject playerInfoTemplate;
    [SerializeField] private Transform playersContainer;

    [SerializeField] private TMP_InputField lobbyInput;
    [SerializeField] private TextMeshProUGUI lobbyTitle;
    [SerializeField] private TextMeshProUGUI lobbyIDText;

    [SerializeField] private Button startGameButton;

    public Dictionary<CSteamID, Transform> playersOnLobbyInfoDictionary = new Dictionary<CSteamID, Transform>();


    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        playerInfoTemplate.SetActive(false);
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
        Instance.lobbyTitle.text = lobbyName;
        Instance.startGameButton.gameObject.SetActive(isHost);
        Instance.lobbyIDText.text = BootstrapManager.CurrentLobbyID.ToString();
        Instance.OpenLobbyScreen();
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
        if (BootstrapManager.Instance.CheckForPlayersReady())
        {
            string[] scenesToClose = new string[] { "MainMenuScene" };
            BootstrapNetworkManager.ChangeNetworkScene("GameScene", scenesToClose);
        }
        else
        {
            Debug.Log("Not all players are ready!!!");
        }
        
    }


    public void FillPlayerInfo(CSteamID steamId, string name, Texture2D texture)
    {
        if (playersOnLobbyInfoDictionary.ContainsKey(steamId))
        {
            playersOnLobbyInfoDictionary[steamId].Find("name").GetComponent<TextMeshProUGUI>().text = name;
            playersOnLobbyInfoDictionary[steamId].Find("image").GetComponent<RawImage>().texture = texture;
        }
        else
        {
            Transform template = Instantiate(playerInfoTemplate.transform, playersContainer);
            template.Find("name").GetComponent<TextMeshProUGUI>().text = name;
            template.Find("image").GetComponent<RawImage>().texture = texture;
            template.gameObject.SetActive(true);
            if (SteamUser.GetSteamID() != steamId)
            {
                template.Find("readyToggle").GetComponent<Toggle>().interactable = false;
            }
            playersOnLobbyInfoDictionary.Add(steamId, template);
        }

    }

    public void SetPlayerReadyToogle(Toggle toggle)
    {
        var id = playersOnLobbyInfoDictionary.FirstOrDefault(x => x.Value == toggle.transform.parent).Key;
        BootstrapManager.Instance.SetReadyStatus(toggle.isOn, id);
    }

    public void SetPlayerReadyToogleOnLobby(CSteamID steamId, bool isReady)
    {
        playersOnLobbyInfoDictionary[steamId].Find("readyToggle").GetComponent<Toggle>().isOn = isReady;
    }
}
