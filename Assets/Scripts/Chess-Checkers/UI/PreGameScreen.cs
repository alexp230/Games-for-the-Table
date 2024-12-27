using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PreGameScreen : NetworkBehaviour
{
    [SerializeField] private ChessBoard ChessBoard_S;
    [SerializeField] private FormationSelectionScreen FormationScreen;

    [SerializeField] private Toggle ShowValidMovesToggle;
    [SerializeField] private Toggle EnableBoardRotationToggle;

    [SerializeField] private Toggle ForceJumpToggle;
    [SerializeField] private Toggle Player1MovesFirstToggle;
    [SerializeField] private Button ConfirmButton;

    [SerializeField] private GameObject GameScreen;
    [SerializeField] private GameObject OptionsMenu;

    private bool IsLocalGame = BoardMaterials.IsLocalGame;

    void OnEnable()
    {
        FormationScreen.gameObject.SetActive(BoardMaterials.GameType == BoardMaterials.CHECKERS_CHESS_GAME);
        
        SetToggleValues();

        if (!IsLocalGame && NetworkManager.Singleton.LocalClientId != 0)
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

    private void SetToggleValues()
    {
        ShowValidMovesToggle.isOn = BoardMaterials.ShowValidMoves;
        EnableBoardRotationToggle.isOn = BoardMaterials.RotateBoardOnMove;
        ForceJumpToggle.isOn = BoardMaterials.ForceJump;
        Player1MovesFirstToggle.isOn = true;

        BoardMaterials.IsPaused = true;

        if (IsLocalGame)
            EnableBoardRotationToggle.gameObject.SetActive(true);
        else
            EnableBoardRotationToggle.gameObject.SetActive(false);

        if (BoardMaterials.GameType == BoardMaterials.CHESS_GAME)
            ForceJumpToggle.gameObject.SetActive(false);
        else
            ForceJumpToggle.gameObject.SetActive(true);
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
        BoardMaterials.RotateBoardOnMove = IsLocalGame ? EnableBoardRotationToggle.isOn : false;

        PlayerData.PlayerID = GetPlayerID(Player1MovesFirstToggle.isOn);

        this.gameObject.SetActive(false);
        TurnOnGameScreen();

        if (BoardMaterials.GameType == BoardMaterials.CHECKERS_CHESS_GAME)
            FormationScreen.OnBeginSelection();
        else
        {
            GameScreen.SetActive(true);
            ChessBoard_S.StartGame();
        }
    }

    private int GetPlayerID(bool isPlayer1)
    {
        if (IsLocalGame)
            return isPlayer1 ? 0 : 1;
        else
            return (isPlayer1 == (NetworkManager.Singleton.LocalClientId == 0)) ? 0 : 1;  
    }

    private void TurnOnGameScreen()
    {
        foreach (Transform child in GameScreen.transform)
            if (child.name.Contains("ToolTip"))
                child.gameObject.SetActive(BoardMaterials.GameType == BoardMaterials.CHECKERS_CHESS_GAME);
    }
}
