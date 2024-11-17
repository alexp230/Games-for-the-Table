using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSelection : MonoBehaviour
{    
    public void OnCheckersButton()
    {
        BoardMaterials.GameType = BoardMaterials.CHECKERS_GAME;
        BoardMaterials.IsLocalGame = true;
        SceneManager.LoadScene(sceneName:"Chess-Checkers");
    }

    public void OnChessButton()
    {
        BoardMaterials.GameType = BoardMaterials.CHESS_GAME;
        BoardMaterials.IsLocalGame = true;
        SceneManager.LoadScene(sceneName:"Chess-Checkers");
    }
}
