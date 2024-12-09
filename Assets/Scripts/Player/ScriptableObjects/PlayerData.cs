using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public static string PlayerName = "Player";
    public static int PlayerID = -1;
    public static string LobbyID = "";

    public static int[] PawnTokens = new int[2] { 0,0 };
    public static int[] KnightTokens = new int[2] { 0,0 };
    public static int[] BishopTokens = new int[2] { 0,0 };
    public static int[] RookTokens = new int[2] { 0,0 };
    public static int[] QueenTokens = new int[2] { 0,0 };
    public static bool HasTokens(int id)
    {
        return (PawnTokens[id] + KnightTokens[id] + BishopTokens[id] + RookTokens[id] + QueenTokens[id]) == 0;
    }

}
