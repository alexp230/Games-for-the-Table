using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreen : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI PlayerWinner;

    // ChessBoard(OnWinnerAnnounced) UE
    public void SetWinnerText(string winnerText)
    {
        PlayerWinner.text = winnerText; 
    }

    // Canvas/VictoryScreen(MainMenuButton) OC
    public void OnMainMenuButton()
    {
        NetworkManager.Singleton?.Shutdown();
        SceneManager.LoadScene(sceneName:"MainMenu");
    }
}
