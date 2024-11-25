using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsScreen : MonoBehaviour
{
    [SerializeField] private ChessBoard ChessBoard_S;
    [SerializeField] private Toggle ShowValidMovesToggle;
    [SerializeField] private Toggle EnableBoardRotationToggle;

    void OnEnable()
    {
        ShowValidMovesToggle.isOn = BoardMaterials.ShowValidMoves;
        EnableBoardRotationToggle.isOn = BoardMaterials.RotateBoardOnMove;
        BoardMaterials.IsPaused = true;

        if (BoardMaterials.IsLocalGame)
            EnableBoardRotationToggle.gameObject.SetActive(true);
        else
            EnableBoardRotationToggle.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        BoardMaterials.ShowValidMoves = ShowValidMovesToggle.isOn;
        BoardMaterials.RotateBoardOnMove = EnableBoardRotationToggle.isOn;
        BoardMaterials.IsPaused = false;

        if (BoardMaterials.RotateBoardOnMove)
            ChessBoard_S.SetCameraAndPiecesRotation();
        else
            ChessBoard_S.FixCameraAndPiecesRotation();
    }

    public void OnResignButton()
    {
        NetworkManager.Singleton?.Shutdown();
        SceneManager.LoadScene(sceneName:"MainMenu");
    }
}
