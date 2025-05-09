using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    [SerializeField] private TMP_Dropdown GameModeSelectionDropDown;
    [SerializeField] private TextMeshProUGUI GameModeText;
    [SerializeField] private TextMeshProUGUI LobbyCodeText;
    [SerializeField] private Toggle IsPrivateLobbyToggle;
    [SerializeField] private Button StartGameButton;
    [SerializeField] private Button CreateLobbyButton;
    [SerializeField] private Button LeaveLobbyButton;
    [SerializeField] private TMP_InputField JoinLobbyInputField;
    [SerializeField] private Button JoinLobbyButton;
    [SerializeField] private Button BackButton;

    private Lobby HostLobby = null;
    private Lobby JoinedLobby = null;
    private float HeartBeatTimer = 15f;
    private float LobbyUpdateTimer = 3f;
    private float JoinLobbyTimer = 4f;
    private float LobbyUIUpdateTimer = 2.5f;
    private float StartGameTimer = -999f;

    void Start()
    {
        SignIn();
        ResetLobbyVariables();
        ShutDownNetworkManagerAndRelay();
    }
    private async void SignIn()
    {
        string playerName;

        string extension = UnityEngine.Random.Range(1, 99999).ToString().PadLeft(5, '0');
        playerName = $"Player{extension}";
        
        // try{
        //     playerName = SteamIntegration.GetSteamName();
        // }
        // catch (Exception e){
        //     string extension = UnityEngine.Random.Range(1, 99999).ToString().PadLeft(5, '0');
        //     playerName = $"Player{extension}";

        //     print(e);
        // }
        
        PlayerData.PlayerName = playerName;

        InitializationOptions options = new InitializationOptions();
        options.SetProfile(playerName);
        await UnityServices.InitializeAsync(options);

        if (AuthenticationService.Instance.IsSignedIn)
            return;

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        if (AuthenticationService.Instance.IsSignedIn)
            print($"Signed in {AuthenticationService.Instance.PlayerId}");
        
        print(playerName);
    }
    private async void ResetLobbyVariables()
    {
        try{
            ClearLocalLobbyData();
            FillPlayerList(JoinedLobby);

            if (string.IsNullOrEmpty(PlayerData.LobbyID))
                return;

            Lobby currentLobby = await LobbyService.Instance.GetLobbyAsync(PlayerData.LobbyID);
            string myPlayerId = AuthenticationService.Instance.PlayerId;

            foreach (Player player in currentLobby.Players)
            {
                if (player.Id == myPlayerId)
                {
                    JoinedLobby = currentLobby;
                    if (myPlayerId == currentLobby.HostId)
                        HostLobby = currentLobby;
                    return;
                }
            }
        }
        catch (Exception e){
            print(e);
        }
        FillPlayerList(JoinedLobby);
    }
    private void ShutDownNetworkManagerAndRelay()
    {
        NetworkManager.Singleton?.Shutdown();
    }

    void OnDisable()
    {
        if (HostLobby != null)
            DeleteLobby();
        else if (JoinedLobby != null)
            LeaveLobby();
    }

    void FixedUpdate()
    {
        HandleHeartBeat();
        HandleLobbyPullForUpdates();
        HandleDelayingJoinByCodeButton();
        HandleUIUpdates();

        BeginGameWhenApplicable();        
    }
    private async void HandleHeartBeat()
    {
        if (HostLobby != null)
        {
            HeartBeatTimer -= Time.fixedDeltaTime;
            if (HeartBeatTimer <= 0f)
            {
                float heartbeatTimerMax = 15.3f;
                HeartBeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(HostLobby.Id);
            }
        }
    }
    private async void HandleLobbyPullForUpdates()
    {
        if (JoinedLobby != null)
        {
            LobbyUpdateTimer -= Time.fixedDeltaTime;
            if (LobbyUpdateTimer <= 0f)
            {
                float lobbyUpdateTimerMax = 2.2f;
                LobbyUpdateTimer = lobbyUpdateTimerMax;

                try{

                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
                    JoinedLobby = lobby;

                    if (IfKickedFromLobby())
                        ClearLocalLobbyData();
                    else
                    {
                        CheckForHostChange();
                        SetMiscLobbyVariables();
                        CheckIfHostStartedGame();   
                        FillPlayerList(lobby);
                    }
                }
                catch{
                    ClearLocalLobbyData();
                }
            }
        }
    }
    private void HandleUIUpdates()
    {
        LobbyUIUpdateTimer -= Time.fixedDeltaTime;
        if (LobbyUIUpdateTimer > 0f)
            return;
        
        if (JoinedLobby != null && JoinedLobby.Data[START_GAME].Value != "0") // If game started
        {
            GameModeSelectionDropDown.gameObject.SetActive(false);
            GameModeText.gameObject.SetActive(false);
            LobbyCodeText.gameObject.SetActive(false);
            IsPrivateLobbyToggle.gameObject.SetActive(false);
            StartGameButton.gameObject.SetActive(false);

            CreateLobbyButton.gameObject.SetActive(false);
            LeaveLobbyButton.gameObject.SetActive(false);
            JoinLobbyInputField.gameObject.SetActive(false);
            JoinLobbyButton.gameObject.SetActive(false);
            BackButton.gameObject.SetActive(false);

        }
        else if (HostLobby == null && JoinedLobby == null)
        {
            GameModeSelectionDropDown.gameObject.SetActive(true);
            GameModeText.gameObject.SetActive(false);
            LobbyCodeText.gameObject.SetActive(false);
            IsPrivateLobbyToggle.gameObject.SetActive(true);
            StartGameButton.gameObject.SetActive(false);

            CreateLobbyButton.gameObject.SetActive(true);
            LeaveLobbyButton.gameObject.SetActive(false);
            JoinLobbyInputField.gameObject.SetActive(true);
            JoinLobbyButton.gameObject.SetActive(true);
            BackButton.gameObject.SetActive(true);
        }
        else if (HostLobby != null)
        {
            GameModeSelectionDropDown.gameObject.SetActive(true);
            GameModeText.gameObject.SetActive(false);
            LobbyCodeText.gameObject.SetActive(true);
            IsPrivateLobbyToggle.gameObject.SetActive(true);
            StartGameButton.gameObject.SetActive((HostLobby.Players.Count >= 2) ? true : false);

            CreateLobbyButton.gameObject.SetActive(false);
            LeaveLobbyButton.gameObject.SetActive(true);
            JoinLobbyInputField.gameObject.SetActive(false);
            JoinLobbyButton.gameObject.SetActive(false);
            BackButton.gameObject.SetActive(false);
        }
        else if (JoinedLobby != null)
        {
            GameModeSelectionDropDown.gameObject.SetActive(false);
            GameModeText.gameObject.SetActive(true);
            LobbyCodeText.gameObject.SetActive(true);
            IsPrivateLobbyToggle.gameObject.SetActive(false);
            StartGameButton.gameObject.SetActive(false);

            CreateLobbyButton.gameObject.SetActive(false);
            LeaveLobbyButton.gameObject.SetActive(true);
            JoinLobbyInputField.gameObject.SetActive(false);
            JoinLobbyButton.gameObject.SetActive(false);
            BackButton.gameObject.SetActive(false);
        }

        float LobbyUIUpdateTimerMax = 1;
        LobbyUIUpdateTimer = LobbyUIUpdateTimerMax;
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
    private void SetMiscLobbyVariables()
    {
        IsPrivateLobbyToggle.isOn = JoinedLobby.IsPrivate;
        LobbyCodeText.text = JoinedLobby.LobbyCode;
        GameModeText.text = JoinedLobby.Data[GAME_MODE].Value;
    }
    private void CheckIfHostStartedGame()
    {
        if (JoinedLobby != null && JoinedLobby.Data[START_GAME].Value != "0" && HostLobby == null)
        {
            JoinRelay(JoinedLobby.Data[START_GAME].Value);
            LeaveLobby();
        }
    }
    private void HandleDelayingJoinByCodeButton()
    {
        JoinLobbyTimer -= Time.fixedDeltaTime;
        if (JoinLobbyTimer < 0f)
            JoinLobbyTimer = 0f;
    }

    private void BeginGameWhenApplicable()
    {
        if (StartGameTimer < -1f)
            return;
        
        StartGameTimer -= Time.fixedDeltaTime;

        if (StartGameTimer < 0f && HostLobby != null)
        {
            LeaveLobby();

            string sceneName = SetLocalPlayerGameMode();
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }




    [Command]
    private async Task<string> CreateRelay()
    {
        try{
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(7);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            print("JoinCode: " + joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            StartGameTimer = 5f;

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
            SetLocalPlayerGameMode();

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
    public async void OnStartGameButton()
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





    [Command]
    private async void CreateLobby()
    {
        try {
            if (HostLobby != null || JoinedLobby != null)
                return;
            
            string lobbyName = "MyLobby";
            int maxPlayers = 8;
            string gameMode = GameModeSelectionDropDown.options[GameModeSelectionDropDown.value].text;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions {
                IsPrivate = IsPrivateLobbyToggle.isOn,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject> {
                    { GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode) },
                    { START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0")}}
                };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            HostLobby = lobby;
            JoinedLobby = HostLobby;
            PlayerData.LobbyID = lobby.Id;

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
            if (HostLobby != null || JoinedLobby != null)
                return;

            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions() {
                Player = GetPlayer(),
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            JoinedLobby = lobby;
            PlayerData.LobbyID = lobby.Id;

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
            if (HostLobby != null || JoinedLobby != null)
                return;

            QuickJoinLobbyOptions quickJoinLobbyByCodeOptions = new QuickJoinLobbyOptions() {
                Player = GetPlayer(),
            };

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyByCodeOptions);
            JoinedLobby = lobby;
            PlayerData.LobbyID = lobby.Id;
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
            if (JoinedLobby == null)
                return;

            if (HostLobby != null && JoinedLobby.Players.Count > 1)
                MigrateLobbyHost();

            await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);

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
            if (HostLobby == null)
                return;

            await LobbyService.Instance.DeleteLobbyAsync(JoinedLobby.Id);

            ClearLocalLobbyData();
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
        if (lobby == null)
        {
            for (int j=0; j<PlayerList.Length; ++j)
            {
                PlayerList[j].transform.Find(PLAYER_NAME).GetComponent<TextMeshProUGUI>().text = "";
                PlayerList[j].transform.Find(KICK_BUTTON).gameObject.SetActive(active(j));
            }
            return;
        }

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
        LobbyCodeText.text = "";
        
        PlayerData.LobbyID = "";

        FillPlayerList(JoinedLobby);
    }




    public void OnCreateLobbyButton()
    {
        CreateLobby();
    }
    public void OnLeaveLobbyButton()
    {
        LeaveLobby();
    }
    public void OnKickPlayerButton(int index)
    {
        KickPlayer(index);
    }
    public void OnJoinLobbyByCodeButton()
    {
        string lobbyCode = JoinLobbyInputField.text.ToUpper();
        if (lobbyCode == "" || lobbyCode.Length != 6 || JoinLobbyTimer > 0.1f)
            return;

        float JoinLobbyTimerMax = 4.0f;
        JoinLobbyTimer = JoinLobbyTimerMax;

        JoinLobbyByCode(lobbyCode);
        JoinLobbyInputField.text = "";
    }
    public async void OnGameModeChange()
    {
        if (HostLobby == null)
            return;

        string gameMode = GameModeSelectionDropDown.options[GameModeSelectionDropDown.value].text;
        Lobby newHostLobby = await Lobbies.Instance.UpdateLobbyAsync(HostLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject> {
                    { GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode)}
                }
            });
        UpdateLobbySetting(newHostLobby);
    }
    public async void OnIsPrivateChange()
    {
        if (HostLobby == null)
            return;
        
        Lobby newHostLobby = await Lobbies.Instance.UpdateLobbyAsync(HostLobby.Id, new UpdateLobbyOptions {
                IsPrivate = IsPrivateLobbyToggle.isOn,
            });
        UpdateLobbySetting(newHostLobby);
    }

    private string SetLocalPlayerGameMode()
    {
        string gameMode = JoinedLobby.Data[GAME_MODE].Value;
        switch (gameMode)
        {
            case "Checkers": case "Chess": case "Combination": BoardMaterials.SetGameType(gameMode); return "Chess-Checkers";
            default: return gameMode;
        }
    }




    [Command]
    private void PrintDataLobby()
    {
        print("HostLobby: " + (HostLobby != null));
        print("JoinedLobby: " + (JoinedLobby != null));
        print("IsHost: " + NetworkManager.Singleton.IsHost);
        print("IsClient: " + (!NetworkManager.Singleton.IsHost && NetworkManager.Singleton.IsClient));
    }
}
