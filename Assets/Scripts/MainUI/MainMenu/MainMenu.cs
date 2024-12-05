using UnityEngine;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        SteamAchievements.UnlockAchievement("Test");
    }
    public void OnQuitGameButton()
    {
        Application.Quit();
    }

}
