using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PreGameScreen : NetworkBehaviour
{
    [SerializeField] private ChessBoard ChessBoard_S;

    [SerializeField] private Toggle ShowValidMovesToggle;
    [SerializeField] private Toggle ForceJumpToggle;
    [SerializeField] private Toggle Player1MovesFirstToggle;
    [SerializeField] private Button ConfirmButton;
    [SerializeField] private GameObject GameScreen;

    private bool IsLocalGame = BoardMaterials.IsLocalGame;

    void Start()
    {
        ChessBoard_S = GameObject.Find("ChessBoard").GetComponent<ChessBoard>();
    }
    void OnEnable()
    {
        SetBoardVariables();

        if (!IsLocalGame)
            if (NetworkManager.Singleton.LocalClientId != 0)
                SetSettingsInteractable(false);

    }

    void OnDisable()
    {
        SetSettingsInteractable(true);
    }


    private void SetSettingsInteractable(bool interactable)
    {
        ForceJumpToggle.interactable = interactable;
        Player1MovesFirstToggle.interactable = interactable;
        ConfirmButton.interactable = interactable;

        ConfirmButton.GetComponentInChildren<TextMeshProUGUI>().text = interactable ? "Confirm" : "Waiting on Host";
    }

    private void SetBoardVariables()
    {
        BoardMaterials.IsPaused = true;
        ShowValidMovesToggle.isOn = BoardMaterials.ShowValidMoves;
        BoardMaterials.ForceJump = ForceJumpToggle.isOn;
        BoardMaterials.IsP1Turn = Player1MovesFirstToggle.isOn;
    }

    public void OnForceJumpToggle()
    {
        if (!IsLocalGame)
            SendForceJump_ClientRPC(ForceJumpToggle.isOn);
    }
    public void OnPlayer1MovesFirstToggle()
    {
        if (!IsLocalGame)
            SendPlayer1MovesFirst_ClientRPC(Player1MovesFirstToggle.isOn);
    }
    public void OnConfirmButton()
    {
        if (!IsLocalGame)
            SetGameDataAndStartGame_ClientRPC();
        else
            StartGame();
    }



    [ClientRpc]
    private void SendForceJump_ClientRPC(bool value)
    {
        if (!BoardMaterials.IsLocalGame && NetworkManager.Singleton.LocalClientId != 0)
            ForceJumpToggle.isOn = value;
    }
    [ClientRpc]
    private void SendPlayer1MovesFirst_ClientRPC(bool value)
    {
        if (!BoardMaterials.IsLocalGame && NetworkManager.Singleton.LocalClientId != 0)
            Player1MovesFirstToggle.isOn = value;
    }
    [ClientRpc]
    private void SetGameDataAndStartGame_ClientRPC()
    {
        StartGame();
    }

    private void StartGame()
    {
        BoardMaterials.IsPaused = false;
        BoardMaterials.ShowValidMoves = ShowValidMovesToggle.isOn;
        BoardMaterials.ForceJump = ForceJumpToggle.isOn;
        BoardMaterials.IsP1Turn = Player1MovesFirstToggle.isOn;

        this.gameObject.SetActive(false);
        GameScreen.SetActive(true);

        ChessBoard_S.StartGame();
    }

}
