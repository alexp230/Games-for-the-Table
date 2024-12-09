using System.Collections.Generic;
using UnityEngine;

public class CombinationGame : MonoBehaviour
{
    private int[] PawnTokenCount = new int[2] { 0,0 };
    private int[] BishopTokenCount = new int[2] { 0,0 };
    private int[] QueenTokenCount = new int[2] { 0,0 };

    public List<string> MoveList = new List<string>();

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            print($"Pawn Tokens p1: {PlayerData.PawnTokens[0]}");
            print($"Pawn Tokens p2: {PlayerData.PawnTokens[1]}");
        }
        if (Input.GetKeyUp(KeyCode.B))
        {
            print($"Bishop Tokens p1: {PlayerData.BishopTokens[0]}");
            print($"Bishop Tokens p2: {PlayerData.BishopTokens[1]}");
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            print($"Knight Tokens p1: {PlayerData.KnightTokens[0]}");
            print($"Knight Tokens p2: {PlayerData.KnightTokens[1]}");
        }
        if (Input.GetKeyUp(KeyCode.R))
        {
            print($"Rook Tokens p1: {PlayerData.RookTokens[0]}");
            print($"Rook Tokens p2: {PlayerData.RookTokens[1]}");
        }
        if (Input.GetKeyUp(KeyCode.Q))
        {
            print($"Queen Tokens p1: {PlayerData.QueenTokens[0]}");
            print($"Queen Tokens p2: {PlayerData.QueenTokens[1]}");
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

        int playerID = BoardMaterials.IsP1Turn ? 0 : 1;
        if (char.ToLower(move[0]) == 'c')
        {
            ProcessCheckerToken(ref PawnTokenCount[playerID], 2, ref PlayerData.PawnTokens[playerID]);
            ProcessCheckerToken(ref BishopTokenCount[playerID], 3, ref PlayerData.BishopTokens[playerID]);
            ProcessCheckerToken(ref QueenTokenCount[playerID], 7, ref PlayerData.QueenTokens[playerID]);

            if (move.Length > 5) // Piece made more than one capture
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
}
