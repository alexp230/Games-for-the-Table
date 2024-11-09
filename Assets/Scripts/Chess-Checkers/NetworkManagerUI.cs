using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : NetworkBehaviour
{
    private static ChessBoard ChessBoard_S;

    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    private bool StartedGame;

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

        StartedGame = false;
    }

    void Start()
    {
        ChessBoard_S = GameObject.Find("ChessBoard").GetComponent<ChessBoard>();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Q))
        {
            int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
            print($"PlayerCount: {playerCount}");
            print(IsHost);

            if (IsHost && !StartedGame)
            {
                StartedGame = true;
                if (playerCount == 1)
                    StartGame_ClientRPC();
                else if (playerCount == 2)
                    StartGame_ClientRPC();
            }
        }
    }

    [ClientRpc]
    private void StartGame_ClientRPC()
    {
        ChessBoard_S.StartGame();
    }
}
