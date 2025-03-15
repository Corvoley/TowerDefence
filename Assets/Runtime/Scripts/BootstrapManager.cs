using FishNet;
using FishNet.Managing;
using FishNet.Object;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapManager : MonoBehaviour
{
    public static BootstrapManager Instance;

    public static Action OnHostDisconnected;

    [SerializeField] private string menuSceneName = "MainMenuScene";
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private FishySteamworks.FishySteamworks fishySteamworks;


    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;
    protected Callback<LobbyChatUpdate_t> LobbyChatUpdate;
    protected Callback<LobbyDataUpdate_t> LobbyDataUpdate;

    protected Callback<AvatarImageLoaded_t> AvatarImageLoaded;

   
    public static ulong CurrentLobbyID;
    private CSteamID originalHostId;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        LobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);

        AvatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
        

    }

    private void OnLobbyDataUpdate(LobbyDataUpdate_t callback)
    {
        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        CSteamID playerId = new CSteamID(callback.m_ulSteamIDMember);

        if (lobbyId == new CSteamID(CurrentLobbyID))
        {
            string steamId = playerId.ToString();
            string isReadyKey = steamId + "_isReady";
            string isReadyValue = SteamMatchmaking.GetLobbyMemberData(lobbyId, playerId, isReadyKey);

            if (bool.TryParse(isReadyValue, out bool isReady))
            {
                Debug.Log($"Player {SteamFriends.GetFriendPersonaName(playerId)} is ready: {isReady}");
                MainMenuManager.Instance.SetPlayerReadyToogleOnLobby(playerId, isReady);
            }
        }
    }

    public bool CheckForPlayersReady()
    {
        bool allReady = false;
        int numMembers = SteamMatchmaking.GetNumLobbyMembers(new CSteamID(CurrentLobbyID));

        for (int i = 0; i < numMembers; i++)
        {
            CSteamID playerId = SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(CurrentLobbyID), i);
            string playerName = SteamFriends.GetFriendPersonaName(playerId);
            string steamId = playerId.ToString();
            string isReadyKey = steamId + "_isReady";
            string isReadyValue = SteamMatchmaking.GetLobbyMemberData(new CSteamID(CurrentLobbyID), playerId, isReadyKey);

            if (bool.TryParse(isReadyValue, out bool isReady))
            {
                if (isReady)
                {
                    allReady = true;
                }
                else
                {
                    allReady = false;
                    break;
                }

            }
        }
        return allReady;

    }
    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        UpdatePlayerList();
    }

    public void OpenMenuScene()
    {
        SceneManager.LoadScene(menuSceneName, LoadSceneMode.Additive);
    }

    public static void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
    }
    public void SetReadyStatus(bool isReady, CSteamID id)
    {
        if (SteamUser.GetSteamID() != id) return;
        string steamId = SteamUser.GetSteamID().ToString();
        SteamMatchmaking.SetLobbyMemberData(new CSteamID(CurrentLobbyID), steamId + "_isReady", isReady.ToString());
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        Debug.Log("Starting lobby creation: " + callback.m_eResult);
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            return;
        }
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(CurrentLobbyID), "name", SteamFriends.GetPersonaName().ToString() + "'s lobby");

        fishySteamworks.SetClientAddress(SteamUser.GetSteamID().ToString());
        fishySteamworks.StartConnection(true);
        Debug.Log("Lobby creation was successful");





    }
    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        CurrentLobbyID = callback.m_ulSteamIDLobby;

        MainMenuManager.LobbyEntered(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name"), networkManager.IsServerStarted);


        fishySteamworks.SetClientAddress(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "HostAddress"));
        fishySteamworks.StartConnection(false);     

        UpdatePlayerList();
    }


    private void UpdatePlayerList()
    {
        int numMembers = SteamMatchmaking.GetNumLobbyMembers(new CSteamID(CurrentLobbyID));

        for (int i = 0; i < numMembers; i++)
        {
            CSteamID playerId = SteamMatchmaking.GetLobbyMemberByIndex(new CSteamID(CurrentLobbyID), i);
            string playerName = SteamFriends.GetFriendPersonaName(playerId);
            var imageId = SteamFriends.GetLargeFriendAvatar(playerId);

            if (imageId != -1)
            {
                var texture = GetSteamImageAsTexture(imageId);
                Debug.Log("Player " + i + ": " + playerName);
                MainMenuManager.Instance.FillPlayerInfo(playerId, SteamFriends.GetFriendPersonaName(playerId), texture);
            }
            else
            {
                MainMenuManager.Instance.FillPlayerInfo(playerId, SteamFriends.GetFriendPersonaName(playerId), null);
            }

        }
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;
        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            byte[] buffer = new byte[width * height * 4];
            isValid = SteamUtils.GetImageRGBA(iImage, buffer, (int)(width * height * 4));
            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(buffer);
                texture.Apply();
            }
        }
        return texture;
    }
    private void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
    {
        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        CSteamID playerId = new CSteamID(callback.m_ulSteamIDUserChanged);

        // Check if the change is in the current lobby
        if (lobbyId == new CSteamID(CurrentLobbyID))
        {
            // Determine the type of change
            switch (callback.m_rgfChatMemberStateChange)
            {
                case (uint)EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                    Debug.Log("Player joined the lobby: " + SteamFriends.GetFriendPersonaName(playerId));
                    NotifyPlayerJoined(playerId);
                    UpdatePlayerList();
                    break;

                case (uint)EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                    Debug.Log("Player left the lobby: " + SteamFriends.GetFriendPersonaName(playerId));
                    if (playerId == originalHostId)
                    {
                        Debug.Log("Host has left the lobby... Closing match ");
                        OnHostDisconnected?.Invoke();
                    }
                    break;

                case (uint)EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                    Debug.Log("Player disconnected from the lobby: " + SteamFriends.GetFriendPersonaName(playerId));
                    break;

                case (uint)EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                    Debug.Log("Player was kicked from the lobby: " + SteamFriends.GetFriendPersonaName(playerId));
                    break;

                case (uint)EChatMemberStateChange.k_EChatMemberStateChangeBanned:
                    Debug.Log("Player was banned from the lobby: " + SteamFriends.GetFriendPersonaName(playerId));
                    break;

            }
        }
    }



    private void NotifyPlayerJoined(CSteamID newPlayerId)
    {
        string newPlayerName = SteamFriends.GetFriendPersonaName(newPlayerId);
        string message = $"Player {newPlayerName} has joined the lobby!";

        // Broadcast the message to all players in the lobby
        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
        SteamMatchmaking.SendLobbyChatMsg(new CSteamID(CurrentLobbyID), messageBytes, messageBytes.Length);
    }

    public void OnLobbyChatMessageReceived(LobbyChatMsg_t callback)
    {
        byte[] messageBytes = new byte[4096];
        int bytesRead = SteamMatchmaking.GetLobbyChatEntry(new CSteamID(CurrentLobbyID), (int)callback.m_iChatID, out CSteamID senderId, messageBytes, messageBytes.Length, out EChatEntryType chatEntryType);

        if (bytesRead > 0)
        {
            string message = System.Text.Encoding.UTF8.GetString(messageBytes, 0, bytesRead);
            Debug.Log("Received lobby chat message: " + message);
        }
    }
    public async static void JoinByID(CSteamID steamID)
    {
        Debug.Log("Attempting to join lobby with ID: " + steamID.m_SteamID);
        if (SteamMatchmaking.RequestLobbyData(steamID))
        {
            await Instance.SetOriginalHostID(steamID);
            SteamMatchmaking.JoinLobby(steamID);
        }
        else
        {
            Debug.Log("Failed to join lobby with ID: " + steamID.m_SteamID);
        }
    }

    private async Task SetOriginalHostID(CSteamID steamID)
    {
        await Awaitable.WaitForSecondsAsync(0.1f);
        Instance.originalHostId = SteamMatchmaking.GetLobbyOwner(steamID);
        await Awaitable.WaitForSecondsAsync(0.1f);
    }
    public static void LeaveLobby()
    {
        SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
        CurrentLobbyID = 0;

        Instance.fishySteamworks.StopConnection(false);
        if (Instance.networkManager.IsServerStarted)
        {
            Instance.fishySteamworks.StopConnection(true);
        }

    

    }
}
