using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombinationGame : MonoBehaviour
{
    [SerializeField] private BoardMaterials BoardMaterials_SO;

    private const int P_MAX = 2;
    private const int B_MAX = 3;
    private const int Q_MAX = 7;
    private int[] PawnTokenCount = new int[2] { 0,0 };
    private int[] BishopTokenCount = new int[2] { 0,0 };
    private int[] QueenTokenCount = new int[2] { 0,0 };

    public List<string> MoveList = new List<string>();

    [SerializeField] private TextMeshProUGUI PawnTokens;
    [SerializeField] private TextMeshProUGUI BishopTokens;
    [SerializeField] private TextMeshProUGUI KnightTokens;
    [SerializeField] private TextMeshProUGUI RookTokens;
    [SerializeField] private TextMeshProUGUI QueenTokens;

    public void AddToMoveList(string move)
    {
        MoveList.Add(move);

        UpdateAndCheckCounters(move);
    }

    private void UpdateAndCheckCounters(string move)
    {
        if ((char.ToLower(move[0]) == 'k') && (move.Length == 3)) // King Spawn
            return;

        int playerID = BoardMaterials.IsP1Turn ? 0 : 1;
        if (char.ToLower(move[0]) == 'c' && !char.IsLetter(move[move.Length-1])) // Checker move but not elevation
        {
            ProcessCheckerToken(ref PawnTokenCount[playerID], P_MAX, ref PlayerData.PawnTokens[playerID]);
            ProcessCheckerToken(ref BishopTokenCount[playerID], B_MAX, ref PlayerData.BishopTokens[playerID]);
            ProcessCheckerToken(ref QueenTokenCount[playerID], Q_MAX, ref PlayerData.QueenTokens[playerID]);

            if (move.Length > 6) // Piece made more than one capture
                ++PlayerData.KnightTokens[playerID];
            
            char newRowPos = move[move.Length-1]; // Checker promotes to Rook
            if ((BoardMaterials.IsP1Turn && (newRowPos == '8')) || (!BoardMaterials.IsP1Turn && (newRowPos == '1')))
                ++PlayerData.RookTokens[playerID];
        }
        else
            ResetCheckerTokens(playerID);

    }
    private void ProcessCheckerToken(ref int tokenCount, int threshold, ref int playerToken)
    {
        if (++tokenCount == threshold)
        {
            tokenCount = 0;
            ++playerToken;
        }
    }
    private void ResetCheckerTokens(int id)
    {
        PawnTokenCount[id] = 0;
        BishopTokenCount[id] = 0;
        QueenTokenCount[id] = 0;
    }
    // ChessBoard OnChangedTurn UE
    public void SetText(bool p1Turn)
    {
        int playerID = BoardMaterials.RotateBoardOnMove ? (p1Turn ? 0 : 1) : PlayerData.PlayerID;
        Color color = (playerID == 0) ? BoardMaterials_SO.Piece_p1Color.color : BoardMaterials_SO.Piece_p2Color.color;

        PawnTokens.text = $"P: {PlayerData.PawnTokens[playerID]} [{PawnTokenCount[playerID]}/{P_MAX}]";
        PawnTokens.color = color;
        BishopTokens.text = $"B: {PlayerData.BishopTokens[playerID]} [{BishopTokenCount[playerID]}/{B_MAX}]";
        BishopTokens.color = color;
        KnightTokens.text = $"K: {PlayerData.KnightTokens[playerID]}";
        KnightTokens.color = color;
        RookTokens.text = $"R: {PlayerData.RookTokens[playerID]}";
        RookTokens.color = color;
        QueenTokens.text = $"Q: {PlayerData.QueenTokens[playerID]} [{QueenTokenCount[playerID]}/{Q_MAX}]";
        QueenTokens.color = color;
    }

    // ChessBoard OnGameOver UE
    public void ResetAllTokens()
    {
        for (int id=0; id<2; ++id)
        {
            ResetCheckerTokens(id);
            PlayerData.PawnTokens[id] = 0;
            PlayerData.BishopTokens[id] = 0;
            PlayerData.KnightTokens[id] = 0;
            PlayerData.RookTokens[id] = 0;
            PlayerData.QueenTokens[id] = 0;
        }
        
    }
}
