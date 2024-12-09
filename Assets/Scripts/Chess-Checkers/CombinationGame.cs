using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class CombinationGame : MonoBehaviour
{
    public int PawnTokenCount = 0;
    public int BishopTokenCount = 0;
    public int QueenTokenCount = 0;

    public List<string> MoveList = new List<string>();

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            
        
        print($"Pawn Tokens: {PlayerData.PawnTokens}");
        print($"Bishop Tokens: {PlayerData.BishopTokens}");
        print($"Knight Tokens: {PlayerData.KnightTokens}");
        print($"Rook Tokens: {PlayerData.RookTokens}");
        print($"Queen Tokens: {PlayerData.QueenTokens}");
        print($"PlayerID: {PlayerData.PlayerID}");
        }
    }

    public void AddToMoveList(string move)
    {
        MoveList.Add(move);

        UpdateAndCheckCounters(move);
    }

    private void UpdateAndCheckCounters(string move)
    {
        if ((char.ToLower(move[0]) == 'k') && (move.Length == 3)) // King Spawn
            return;
        if (BoardMaterials.IsP1Turn != (PlayerData.PlayerID == 0))
            return;

        if (char.ToLower(move[0]) == 'c')
        {
            ProcessCheckerToken(ref PawnTokenCount, 2, ref PlayerData.PawnTokens);
            ProcessCheckerToken(ref BishopTokenCount, 3, ref PlayerData.BishopTokens);
            ProcessCheckerToken(ref QueenTokenCount, 7, ref PlayerData.QueenTokens);

            if (move.Length > 5) // Piece made more than one capture
                ++PlayerData.KnightTokens;
            
            char newRowPos = move[move.Length-1]; // Checker promotes to Rook
            if ((BoardMaterials.IsP1Turn && (newRowPos == '8')) || (!BoardMaterials.IsP1Turn && (newRowPos == '1')))
                ++PlayerData.RookTokens;
        }
        else
            ResetCheckerTokens();

    }

    private void ProcessCheckerToken(ref int tokenCount, int threshold, ref int playerToken)
    {
        if (++tokenCount == threshold)
        {
            tokenCount = 0;
            ++playerToken;
        }
    }
    private void ResetCheckerTokens()
    {
        PawnTokenCount = 0;
        BishopTokenCount = 0;
        QueenTokenCount = 0;
    }
}
