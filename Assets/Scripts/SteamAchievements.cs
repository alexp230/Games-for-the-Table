using System;
using UnityEngine;

public class SteamAchievements : MonoBehaviour
{
    public static void UnlockAchievement(string id)
    {
        try{
            Steamworks.Data.Achievement achievement = new Steamworks.Data.Achievement(id);
            achievement.Trigger();
        }
        catch (Exception e){
            print(e);
        }
        
    }
}
