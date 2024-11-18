using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreen : MonoBehaviour
{
    private ChessBoard ChessBoard_S;
    
    void Start()
    {
        ChessBoard_S = GameObject.Find("ChessBoard").GetComponent<ChessBoard>();
    }

    public void OnRestartButton()
    {
        ChessBoard_S.ResetGame();
    }

    public void OnMainMenuButton()
    {
        SceneManager.LoadScene(sceneName:"MainMenu");
    }
}
