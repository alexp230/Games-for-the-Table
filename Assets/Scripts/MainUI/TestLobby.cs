using System;
using System.Collections.Generic;
using System.Threading;
using QFSW.QC;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class TestLobby : MonoBehaviour
{
    [SerializeField] private GameObject[] PlayerList = new GameObject[8];
    private const string PLAYER_NAME = "PlayerName";
    private const string KICK_BUTTON = "KickButton";
    [SerializeField] private TextMeshProUGUI LobbyCode;
    [SerializeField] private TMP_Dropdown GameModeSelection;
    [SerializeField] private Toggle IsPrivateLobbyToggle;

    private Lobby HostLobby;
    private Lobby JoinedLobby;
    private float HeartBeatTimer;
    private float LobbyUpdateTimer;
    private string PlayerName;

    private async void Start()
    {
        string extension = UnityEngine.Random.Range(0, 99999).ToString().PadLeft(5, '0');
        PlayerName = $"Player{extension}";

        InitializationOptions options = new InitializationOptions();
        options.SetProfile(PlayerName);
        await UnityServices.InitializeAsync(options);

        AuthenticationService.Instance.SignedIn += () => {
            print ($"Signed in {AuthenticationService.Instance.PlayerId}");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        print(PlayerName);

        // ------

        ClearLocalLobbyData();
        LobbyCode.text = "";
    }

    private void Update()
    {
        HandleHeartBeat();
        HandleLobbyPullForUpdates();
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
                float lobbyUpdateTimerMax = 4f;
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

                // ---------

                PrintPlayers(lobby);

                // --------

                FillPlayerList(lobby);
                LobbyCode.text = (HostLobby != null) ? lobby.LobbyCode : "";

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
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Checkers") }}
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
                print(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data["GameMode"].Value);
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
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode)}
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
            PlayerName = newPlayerName;

            await LobbyService.Instance.UpdatePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions {
                Data = new Dictionary<string, PlayerDataObject> {
                    { "PlayerName" , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerName)}
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
                { "PlayerName" , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerName)}
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
        print("Players in Lobby " + lobby.Name + " " + lobby.IsPrivate + " " + lobby.Data["GameMode"].Value);
        foreach (Player player in lobby.Players)
            print(player.Id + " " + player.Data["PlayerName"].Value);
    }






    private void FillPlayerList(Lobby lobby)
    {
        int i = -1;
        foreach (Player player in lobby.Players)
            PlayerList[++i].transform.Find(PLAYER_NAME).GetComponent<TextMeshProUGUI>().text = player.Data["PlayerName"].Value;
        
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
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode)}
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
