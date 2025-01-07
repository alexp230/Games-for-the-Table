using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSelection : MonoBehaviour
{
    // CheckerButton UE
    public void OnCheckersButton()
    {
        BoardMaterials.GameType = BoardMaterials.CHECKERS_GAME;
        BoardMaterials.IsLocalGame = true;
        SceneManager.LoadScene(sceneName:"Chess-Checkers");
    }

    // ChessButton UE
    public void OnChessButton()
    {
        BoardMaterials.GameType = BoardMaterials.CHESS_GAME;
        BoardMaterials.IsLocalGame = true;
        SceneManager.LoadScene(sceneName:"Chess-Checkers");
    }

    // CheckerChessButton UE
    public void OnCheckersChessButton()
    {
        BoardMaterials.GameType = BoardMaterials.CHECKERS_CHESS_GAME;
        BoardMaterials.IsLocalGame = true;
        SceneManager.LoadScene(sceneName:"Chess-Checkers");
    }

    // CheckerChessButton UE
    public void OnIchiButton()
    {
        SceneManager.LoadScene(sceneName:"Ichi");
    } 
}
