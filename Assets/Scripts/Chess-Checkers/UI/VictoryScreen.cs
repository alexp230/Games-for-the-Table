using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreen : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI PlayerWinner;

    public void SetWinnerText(string winnerText)
    {
        PlayerWinner.text = winnerText; 
    }

    public void OnMainMenuButton()
    {
        NetworkManager.Singleton?.Shutdown();
        SceneManager.LoadScene(sceneName:"MainMenu");
    }
}
