using System;
using UnityEngine;

public class SteamIntegration : MonoBehaviour
{
    void Start()
    {
        try{
            Steamworks.SteamClient.Init(3219090);
        }
        catch(Exception e){
            // Something went wrong
            //
            //  Steam is closed?
            //  Dont have permission to play app?
            //  Can't find steam_api dll?

            print(e);
        }
    }

    void FixedUpdate()
    {
        Steamworks.SteamClient.RunCallbacks();
    }

    void OnApplicationQuit()
    {
        Steamworks.SteamClient.Shutdown();
    }

    public void IsThisAchievementUnlocked (string id)
    {
        var ach = new Steamworks.Data.Achievement(id);
        Debug.Log($"Achievement {id} status: + {ach.State}");
    }
    
    public void UnlockAchievement(string id)
    {
        var ach = new Steamworks.Data.Achievement(id);
        ach.Trigger();
        Debug.Log($"Achievement {id} unlocked");
    }

    public void ClearAchievementStatus(string id)
    {
        var ach = new Steamworks.Data.Achievement (id);
        ach.Clear();
        Debug.Log($"Achievement {id} cleared");
    }

    public static string GetSteamName()
    {
        char[] Name = Steamworks.SteamClient.Name.ToCharArray();
        
        for (int i=0; i<Name.Length; ++i)
            if (!(char.IsLetterOrDigit(Name[i]) || Name[i] == '-' || Name[i] == '_'))
                Name[i] = '_';
        
        string playerName = new string(Name);
        if (playerName.Length > 30)
            playerName = playerName.Substring(0, 30);
        
        return playerName;
    }

    private void PrintName()
    {
        print(Steamworks.SteamClient.Name);
    }
    private void PrintFriends()
    {
        foreach (Steamworks.Friend friend in Steamworks.SteamFriends.GetFriends())
            print(friend.Name);
    }
}
