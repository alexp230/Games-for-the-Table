using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : NetworkBehaviour
{
    private static ChessBoard ChessBoard_S;

    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    private bool StartedGame = false;

    void Awake()
    {            
        serverBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
        });
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
        });
        clientBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
    }


    void Start()
    {
        ChessBoard_S = GameObject.Find("ChessBoard").GetComponent<ChessBoard>();

        // if (BoardMaterials.IsLocalGame)
        //     NetworkManager.Singleton.StartHost();

        ChessBoard_S.StartGame();
        
        print(PlayerData.PlayerName + " (" + NetworkManager.Singleton.LocalClientId + ") is ready to play");
        
        StartedGame = true;
    }

    void OnDisable()
    {
        NetworkManager.Singleton.Shutdown();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
            print($"PlayerCount: {playerCount}");
            print(IsHost);

            if (IsHost && !StartedGame && playerCount > 0)
            {
                StartedGame = true;
                if (playerCount == 1)
                    StartGame_ClientRPC(isSinglePlayer: true);
                else if (playerCount == 2)
                    StartGame_ClientRPC(isSinglePlayer: false);
            }
        }
    }

    [ClientRpc]
    private void StartGame_ClientRPC(bool isSinglePlayer)
    {
        BoardMaterials.IsLocalGame = isSinglePlayer;
        ChessBoard_S.StartGame();
    }
}
