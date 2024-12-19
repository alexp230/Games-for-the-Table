using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Instructions : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI GameTitle;
    [SerializeField] private TextMeshProUGUI InstructionText;
    [SerializeField] private TextMeshProUGUI InstructionCount;
    [SerializeField] private Button NextButton;
    [SerializeField] private Button PreviousButton;

    public List<string> InstructionsList = new List<string>();

    private static readonly string CHECKERS = @"
Players take turns moving each piece one square forward and diagonally at a time./
If an opponent's piece is diagonally adjacent and the square beyond it is empty, you must jump over and capture the opponent's piece. [Unless 'Force Jump' is turned off]/
When a piece reaches the opponent's back row, it becomes a duke, which can move diagonally both forward and backward./
The game ends when one player captures all opponent pieces or if the opponent has no valid moves./";

    private static readonly string CHESS = @"
The first player to checkmate the enemy king wins./
If the opponent has no legal moves or if 50 moves with no pawn movements or a piece capture has occurred, the game ends in a draw./
If both players agree to a draw, the game ends in a draw/";

    private static readonly string COMBINATION = @$"
In this fusion of checkers and chess, the pieces all move as normal, except the king cannot be checkmated./
Once a formation is chosen, both players take turn moving their pieces./
During your turn, you are able to elevate your checkers into chess pieces by using a chess piece token./
While your mouse is hovering over a checker piece, use the hotkeys to elevate piece.
Hotkeys = ChessPiece
----------------------
p = pawn
b = bishop
k or h = knight
r = rook
q = queen/
<align=center>Pawn</align=center>
To earn a pawn token, you must make {CombinationGame.P_MAX} consecutive checker moves./
<align=center>Knight</align=center>
To earn a knight token, you must capture two or more pieces in one turn with a checker piece./
<align=center>Bishop</align=center>
To earn a bishop token, you must make {CombinationGame.B_MAX} consecutive checker moves./
<align=center>Rook</align=center>
To earn a rook token, you must promote a checker to a duke./
<align=center>Queen</align=center>
To earn a queen token, you must make {CombinationGame.Q_MAX} consecutive checker moves./
After {ChessBoard.KING_SPAWN} moves has passed, each player will summon their king on an unoccupied square their back row./
The first player to capture the opponent's king wins./";

    void OnEnable()
    {
        if (InstructionsList.Count > 0)
            return;

        string directions;
        string title;
        switch (BoardMaterials.GameType)
        {
            case BoardMaterials.CHECKERS_GAME: title = "Checkers"; directions = CHECKERS; break;
            case BoardMaterials.CHESS_GAME: title = "Chess"; directions = CHESS; break;
            case BoardMaterials.CHECKERS_CHESS_GAME: title = "Checkers-Chess"; directions = COMBINATION; break;
            default: title = "Checkers"; directions = CHECKERS; break;
        }

        CreateInstructionList(directions);

        GameTitle.text = title+" Instructions";

        InstructionCount.text = $"1/{InstructionsList.Count}";
        InstructionText.text = InstructionsList[0];
        PreviousButton.interactable = false;
        NextButton.interactable = InstructionsList.Count != 1;
    }

    private void CreateInstructionList(string directions)
    {
        int beginning = 2;
        for (int i=2; i<directions.Length; ++i)
        {
            if (directions[i] == '/' && directions[i-1] != '<')
            {
                InstructionsList.Add(directions.Substring(beginning, i-beginning));

                while ((++i < directions.Length) && (directions[i] == 13 || directions[i] == 10));
                beginning = i;
            }
        }
    }

    public void OnNextButton()
    {
        for (int i=0; i<InstructionsList.Count; ++i)
        {
            if (InstructionText.text == InstructionsList[i])
            {
                ++i;
                InstructionCount.text = $"{i+1}/{InstructionsList.Count}";
                InstructionText.text = InstructionsList[i];
                PreviousButton.interactable = true;
                NextButton.interactable = i < InstructionsList.Count-1;

                return;
            }
        }        
    }
    public void OnPreviousButton()
    {
        for (int i=0; i<InstructionsList.Count; ++i)
        {
            if (InstructionText.text == InstructionsList[i])
            {
                --i;
                InstructionCount.text = $"{i+1}/{InstructionsList.Count}";
                InstructionText.text = InstructionsList[i];
                PreviousButton.interactable = i > 0;
                NextButton.interactable = true;

                return;
            }
        } 
    }
}
