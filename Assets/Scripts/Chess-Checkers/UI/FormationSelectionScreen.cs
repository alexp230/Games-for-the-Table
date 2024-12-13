using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class FormationSelectionScreen : NetworkBehaviour
{
    [SerializeField] private ChessBoard ChessBoard_S;
    [SerializeField] private GameObject GameScreen;

    private readonly Dictionary<string, string> Formations = new Dictionary<string, string>
    {
        {"Apical", BoardMaterials.Apical},
        {"Canal", BoardMaterials.Canal},
        {"Default", BoardMaterials.Default},
        {"Offset", BoardMaterials.Offset},
        {"Pyramid", BoardMaterials.Pyramid},
        {"Wedges", BoardMaterials.Wedges},
        {"Xs", BoardMaterials.Xs},
    };

    [SerializeField] private Toggle IsPlayer1Toggle;
    [SerializeField] private TextMeshProUGUI PlayerTurnText;
    [SerializeField] private Button PlayGameButton;
    [SerializeField] private TextMeshProUGUI P1_FormationText;
    [SerializeField] private TextMeshProUGUI P2_FormationText;

    public void OnBeginSelection()
    {
        foreach (Transform child in this.transform)
            child.gameObject.SetActive(true);
        
        SetInteractableUI();
        OnIsPlayer1ToggleChange();
    }

    void OnEnable()
    {
        BoardMaterials.P1_Formation = "";
        BoardMaterials.P2_Formation = "";
        P1_FormationText.text = "P1: ";
        P2_FormationText.text = "P2: ";

        PlayGameButton.interactable = IsAllPlayersFormationSelected();
        OnIsPlayer1ToggleChange();
    }

    private bool IsAllPlayersFormationSelected()
    {
        return !string.IsNullOrEmpty(BoardMaterials.P1_Formation) && !string.IsNullOrEmpty(BoardMaterials.P2_Formation);
    }

    public void OnFormationSelection(string formation)
    {
        string strategy = Formations[formation];
        if (IsPlayer1Toggle.isOn)
        {
            BoardMaterials.P1_Formation = strategy;
            P1_FormationText.text = $"P1: {formation}";
        }
        else
        {
            BoardMaterials.P2_Formation = reverseString(strategy);
            P2_FormationText.text = $"P2: {formation}";
        }

        SetInteractableUI();

        SendToServer_ServerRPC(PlayerData.PlayerID, formation);
    }

    public void OnPlayGameButton()
    {
        if (!BoardMaterials.IsLocalGame)
            StartGame_ClientRPC();
        else
            StartGame();
        
    }
    private void StartGame()
    {
        this.gameObject.SetActive(false);
        GameScreen.SetActive(true);
        ChessBoard_S.StartGame();
    }

    public void OnIsPlayer1ToggleChange()
    {        
        string player = BoardMaterials.IsLocalGame ? (IsPlayer1Toggle.isOn ? "1" : "2") : (PlayerData.PlayerID==0 ? "1" : "2");
        PlayerTurnText.text = $"Player {player}'s Selection";
    }

    public void SetInteractableUI()
    {
        if (BoardMaterials.IsLocalGame)
        {
            IsPlayer1Toggle.isOn ^= true;
            PlayGameButton.interactable = IsAllPlayersFormationSelected();
        }
        else
        {
            IsPlayer1Toggle.isOn = PlayerData.PlayerID==0;
            IsPlayer1Toggle.interactable = false;
            PlayGameButton.interactable = PlayerData.PlayerID==0 && IsAllPlayersFormationSelected();
        }
    }

    private string reverseString(string text)
    {
        char[] newText = new char[text.Length];
        for (int i=0; i<text.Length; ++i)
            newText[i] = text[text.Length - i - 1];
        
        return new string(newText).ToLower();
    }


    [ServerRpc(RequireOwnership = false)]
    private void SendToServer_ServerRPC(int callerID, string formation)
    {
        print("In Server RPC");
        if (!BoardMaterials.IsLocalGame)
            SendSelection_ClientRPC(callerID, formation);
    }
    [ClientRpc]
    private void SendSelection_ClientRPC(int callerID, string formation)
    {
        print("In Client RPC");
        if (callerID == PlayerData.PlayerID)
            return;

        string strategy = Formations[formation];
        if (callerID == 0)
        {
            BoardMaterials.P1_Formation = strategy;
            P1_FormationText.text = $"P1: {formation}";
        }
        else
        {
            BoardMaterials.P2_Formation = reverseString(strategy);
            P2_FormationText.text = $"P2: {formation}";
        }

        SetInteractableUI();
    }
    [ClientRpc]
    private void StartGame_ClientRPC()
    {
        StartGame();
    }
}
