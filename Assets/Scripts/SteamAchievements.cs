using System.Diagnostics;
using UnityEngine;

public class SteamAchievements : MonoBehaviour
{
    public static void UnlockAchievement(string id)
    {
        Steamworks.Data.Achievement achievement = new Steamworks.Data.Achievement(id);
        achievement.Trigger();
    }
}
