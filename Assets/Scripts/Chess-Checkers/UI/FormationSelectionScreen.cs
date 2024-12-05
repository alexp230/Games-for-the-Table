using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormationSelectionScreen : MonoBehaviour
{
    [SerializeField] private ChessBoard ChessBoard_S;

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

        IsPlayer1Toggle.isOn ^= true;
        PlayGameButton.interactable = IsAllPlayersFormationSelected();

        string reverseString(string text)
        {
            char[] newText = new char[text.Length];
            for (int i=0; i<text.Length; ++i)
                newText[i] = text[text.Length - i - 1];
            
            return new string(newText).ToLower();
        }
    }

    public void OnPlayGameButton()
    {
        this.gameObject.SetActive(false);
        ChessBoard_S.StartGame();
    }

    public void OnIsPlayer1ToggleChange()
    {
        string player = IsPlayer1Toggle.isOn ? "1" : "2";
        PlayerTurnText.text = $"Player {player}'s Selection";
    }
}
