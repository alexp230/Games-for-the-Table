using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreen : NetworkBehaviour
{
    private ChessBoard ChessBoard_S;

    [SerializeField] private TextMeshProUGUI PlayerWinner;
    
    void Start()
    {
        ChessBoard_S = GameObject.Find("ChessBoard").GetComponent<ChessBoard>();
    }

    public void SetWinnerText(string playerName)
    {
        PlayerWinner.text = $"{playerName} Wins!";
    }

    public void OnRestartButton()
    {
        ChessBoard_S.ResetGame();
        this.gameObject.SetActive(false);
    }

    public void OnMainMenuButton()
    {
        NetworkManager.Singleton?.Shutdown();
        SceneManager.LoadScene(sceneName:"MainMenu");
    }
}
