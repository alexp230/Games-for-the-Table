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
        // serverBtn.onClick.AddListener(() => {
        //     NetworkManager.Singleton.StartServer();
        // });
        // hostBtn.onClick.AddListener(() => {
        //     NetworkManager.Singleton.StartHost();
        // });
        // clientBtn.onClick.AddListener(() => {
        //     NetworkManager.Singleton.StartClient();
        // });
    }

    void Start()
    {
        if (BoardMaterials.IsLocalGame)
            NetworkManager.Singleton.StartHost();
            
        ChessBoard_S = GameObject.Find("ChessBoard").GetComponent<ChessBoard>();
        ChessBoard_S.StartGame();

        StartedGame = true;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
            print($"PlayerCount: {playerCount}");
            print(IsHost);

            if (IsHost && !StartedGame)
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
        // BoardMaterials.IsLocalGame = isSinglePlayer;
        ChessBoard_S.StartGame();
    }
}
