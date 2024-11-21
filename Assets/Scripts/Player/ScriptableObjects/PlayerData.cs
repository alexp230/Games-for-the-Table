using Unity.Services.Lobbies.Models;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{

    public static string PlayerName = "A_Player";

    public static string LobbyID = "";
    
}
