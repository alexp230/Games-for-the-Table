using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public static string PlayerName = "Player";
    public static int PlayerID = -1;
    public static string LobbyID = "";

    public static int PawnTokens = 0;
    public static int KnightTokens = 0;
    public static int BishopTokens = 0;
    public static int RookTokens = 0;
    public static int QueenTokens = 0;
    public static bool HasTokens()
    {
        return (PawnTokens + KnightTokens + BishopTokens + RookTokens + QueenTokens) == 0;
    }

}
