using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mono.CSharp;
using QFSW.QC;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestLobby : MonoBehaviour
{
    [SerializeField] private GameObject[] PlayerList = new GameObject[8];
    private const string PLAYER_NAME = "PlayerName";
    private const string KICK_BUTTON = "KickButton";
    private const string GAME_MODE = "GameMode";
    private const string START_GAME = "StartGame";
    [SerializeField] private TextMeshProUGUI LobbyCode;
    [SerializeField] private TMP_Dropdown GameModeSelection;
    [SerializeField] private Toggle IsPrivateLobbyToggle;

    private Lobby HostLobby;
    private Lobby JoinedLobby;
    private float HeartBeatTimer;
    private float LobbyUpdateTimer;

    private async void Start()
    {
        string extension = UnityEngine.Random.Range(0, 99999).ToString().PadLeft(5, '0');
        string playerName = $"Player{extension}";

        InitializationOptions options = new InitializationOptions();
        options.SetProfile(playerName);
        await UnityServices.InitializeAsync(options);

        if (AuthenticationService.Instance.IsSignedIn)
            return;

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        if (AuthenticationService.Instance.IsSignedIn)
            print($"Signed in {AuthenticationService.Instance.PlayerId}");
        
        PlayerData.PlayerName = playerName;
        print(playerName);

        // ------

        ClearLocalLobbyData();
        LobbyCode.text = "";
    }

    private void Update()
    {
        HandleHeartBeat();
        HandleLobbyPullForUpdates();

        BeginGame();        
    }

    private async void HandleHeartBeat()
    {
        if (HostLobby != null)
        {
            HeartBeatTimer -= Time.deltaTime;
            if (HeartBeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                HeartBeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(HostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPullForUpdates()
    {
        if (JoinedLobby != null)
        {
            LobbyUpdateTimer -= Time.deltaTime;
            if (LobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 2f;
                LobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
                JoinedLobby = lobby;

                // ---------
                if (IfKickedFromLobby())
                {
                    ClearLocalLobbyData();
                    return;
                }
                CheckForHostChange();

                if (JoinedLobby.Data[START_GAME].Value != "0")
                {
                    if (HostLobby == null)
                        JoinRelay(JoinedLobby.Data[START_GAME].Value);
                    
                    JoinedLobby = null;
                }

                // ---------

                // PrintPlayers(lobby);

                // --------

                FillPlayerList(lobby);
                LobbyCode.text = (HostLobby != null) ? lobby.LobbyCode : "";

            }
        }
    }

    private void BeginGame()
    {
        if (HostLobby != null && NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            BoardMaterials.GameType = BoardMaterials.CHECKERS_GAME;
            BoardMaterials.IsLocalGame = false;
            NetworkManager.Singleton.SceneManager.LoadScene("Chess-Checkers", LoadSceneMode.Single);
        }
    }




    [Command]
    private async Task<string> CreateRelay()
    {
        try{
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            print("JoinCode: " + joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (RelayServiceException e){
            print(e);
            return null;
        }
    }
 
    [Command]
    private async void JoinRelay(string joinCode)
    {
        try{
            BoardMaterials.GameType = BoardMaterials.CHECKERS_GAME;
            BoardMaterials.IsLocalGame = false;

            print("Joining Relay with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e){
            print(e);
        }
    }

    [Command]
    public async void OnStartGame()
    {
        if (HostLobby != null)
        {
            try{
                print("Start Game");

                string relayCode = await CreateRelay();

                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(JoinedLobby.Id, new UpdateLobbyOptions {
                    Data = new Dictionary<string, DataObject> {
                        { START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });

                UpdateLobbySetting(lobby);
            }
            catch(Exception e){
                print(e);
            }
        }
    }







    private bool IfKickedFromLobby()
    {
        string thisPlayerId = AuthenticationService.Instance.PlayerId;
        foreach (Player player in JoinedLobby.Players)
            if (player.Id == thisPlayerId)
                return false;
        return true;
    }
    private void CheckForHostChange()
    {
        if (JoinedLobby.HostId == AuthenticationService.Instance.PlayerId)
            HostLobby = JoinedLobby;
    }





    [Command]
    private async void CreateLobby()
    {
        try {
            if (HostLobby != null)
                return;
            
            string lobbyName = "MyLobby";
            int maxPlayers = 4;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions {
                IsPrivate = IsPrivateLobbyToggle.isOn,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject> {
                    { GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, "Checkers") },
                    { START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0")}}
                };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            HostLobby = lobby;
            JoinedLobby = HostLobby;

            print("Created Lobby! " + lobbyName + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
            // PrintPlayers(HostLobby);

            FillPlayerList(JoinedLobby);

        }

        catch (Exception e){
            print(e);
        }
    }

    [Command]
    private async void ListLobbies()
    {
        try{
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions() {
                Count = 25,
                Filters = new List<QueryFilter> {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder> {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            print("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
                print(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data[GAME_MODE].Value);
        }
        catch (Exception e){
            print(e);
        }
    }

    [Command]
    private async void JoinLobbyByCode(string lobbyCode)
    {
        try{
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions() {
                Player = GetPlayer(),
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            JoinedLobby = lobby;

            print("Joined Lobby with code: " + lobbyCode);

            PrintPlayers(lobby);
            FillPlayerList(lobby);

        }
        catch (Exception e){
            print(e);
        }
    }

    [Command]
    private async void QuickJoinLobby()
    {
        try {
            QuickJoinLobbyOptions quickJoinLobbyByCodeOptions = new QuickJoinLobbyOptions() {
                Player = GetPlayer(),
            };

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyByCodeOptions);
            JoinedLobby = lobby;
        }
        catch(Exception e){
            print(e);
        }
    }

    [Command]
    private async void UpdateLobbyGameMode(string gameMode)
    {
        try {
            if (HostLobby == null)
                return;

            HostLobby = await Lobbies.Instance.UpdateLobbyAsync(HostLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    { GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode)}
                }
            });

            JoinedLobby = HostLobby;

            PrintPlayers(HostLobby);
        }
        catch (Exception e){
            print(e);
        }
    }

    private void UpdateLobbySetting(Lobby newLobby)
    {
        try {
            if (HostLobby == null)
                return;

            HostLobby = newLobby;
            JoinedLobby = newLobby;

            PrintPlayers(HostLobby);
        }
        catch (Exception e){
            print(e);
        }
    }

    [Command]
    private async void UpdatePlayerName(string newPlayerName)
    {
        try {
            PlayerData.PlayerName = newPlayerName;

            await LobbyService.Instance.UpdatePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions {
                Data = new Dictionary<string, PlayerDataObject> {
                    { PLAYER_NAME , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newPlayerName)}
                }
            });
        }
        catch (Exception e){
            print(e);
        }
    }


    [Command]
    private async void LeaveLobby()
    {
        try {
            if (HostLobby != null && JoinedLobby.Players.Count > 1)
                MigrateLobbyHost();

            await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);

            // -------------------------

            ClearLocalLobbyData();
        }
        catch (Exception e){
            print(e);
        }
    }

    [Command]
    private async void KickPlayer(int index)
    {
        try {
            await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, JoinedLobby.Players[index].Id);
        }
        catch (Exception e){
            print(e);
        }
    }

    [Command]
    private async void MigrateLobbyHost()
    {
        try {
            HostLobby = await Lobbies.Instance.UpdateLobbyAsync(HostLobby.Id, new UpdateLobbyOptions {
                HostId = JoinedLobby.Players[1].Id
            });

            JoinedLobby = HostLobby;
            HostLobby = null;
        }
        catch (Exception e){
            print(e);
        }
    }

    [Command]
    private async void DeleteLobby()
    {
        try {
            await LobbyService.Instance.DeleteLobbyAsync(JoinedLobby.Id);
        }
        catch (LobbyServiceException e){
            print(e);
        }
    }







    private Player GetPlayer()
    {
        Player player = new Player { 
            Data = new Dictionary<string, PlayerDataObject> {
                { PLAYER_NAME , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerData.PlayerName)}
            }
        };
        return player;
    }

    [Command]
    private void PrintPlayers()
    {
        PrintPlayers(JoinedLobby);
    }

    private void PrintPlayers(Lobby lobby)
    {
        print("Players in Lobby " + lobby.Name + " " + lobby.IsPrivate + " " + lobby.Data[GAME_MODE].Value);
        foreach (Player player in lobby.Players)
            print(player.Id + " " + player.Data[PLAYER_NAME].Value);
    }






    private void FillPlayerList(Lobby lobby)
    {
        int i = -1;
        foreach (Player player in lobby.Players)
            PlayerList[++i].transform.Find(PLAYER_NAME).GetComponent<TextMeshProUGUI>().text = player.Data[PLAYER_NAME].Value;
        
        for (int j=i+1; j<PlayerList.Length; ++j)
            PlayerList[j].transform.Find(PLAYER_NAME).GetComponent<TextMeshProUGUI>().text = "";

        for (int j=0; j<PlayerList.Length; ++j)
            PlayerList[j].transform.Find(KICK_BUTTON).gameObject.SetActive(active(j));
        
        bool active(int i)
        {
            return HostLobby != null && i!=0 && PlayerList[i].transform.Find(PLAYER_NAME).GetComponent<TextMeshProUGUI>().text != "";
        }
    }

    private void ClearLocalLobbyData()
    {
        HostLobby = null;
        JoinedLobby = null;
        LobbyCode.text = "";

        // Clear PlayerList Data
        foreach (GameObject playerBar in PlayerList)
        {
            Transform bar = playerBar.transform;
            bar.Find(PLAYER_NAME).GetComponent<TextMeshProUGUI>().text = "";
            bar.Find(KICK_BUTTON).gameObject.SetActive(false);
        }
    }




    public void CreateLobbyButton()
    {
        CreateLobby();
    }
    public void LeaveLobbyButton()
    {
        LeaveLobby();
    }
    public void KickPlayerButton(int index)
    {
        KickPlayer(index);
    }
    public async void OnGameModeChange()
    {
        string gameMode = GameModeSelection.options[GameModeSelection.value].text;
        Lobby newHostLobby = await Lobbies.Instance.UpdateLobbyAsync(HostLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    { GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode)}
                }
            });
        UpdateLobbySetting(newHostLobby);
    }
    public async void OnIsPrivateChange()
    {
        print("Updated Lobby");
        Lobby newHostLobby = await Lobbies.Instance.UpdateLobbyAsync(HostLobby.Id, new UpdateLobbyOptions {
                IsPrivate = IsPrivateLobbyToggle.isOn,
            });
        UpdateLobbySetting(newHostLobby);
    }
}
