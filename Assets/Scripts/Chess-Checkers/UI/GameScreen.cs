using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI PlayerTurnText;
    [SerializeField] private BoardMaterials BoardMaterials_SO;

    private ChessBoard ChessBoard_S;

    void Start()
    {
        ChessBoard_S = GameObject.Find("ChessBoard").GetComponent<ChessBoard>();
        ChessBoard_S.OnChangedTurn += SetPlayerTurnText;
    }

    void OnDisable() 
    {
        ChessBoard_S.OnChangedTurn -= SetPlayerTurnText;
    }

    public void OnResignButton()
    {
        NetworkManager.Singleton?.Shutdown();
        SceneManager.LoadScene(sceneName:"MainMenu");
    }

    private void SetPlayerTurnText(bool isP1Turn)
    {
        PlayerTurnText.text = isP1Turn ? "Light" : "Dark";
        PlayerTurnText.color = isP1Turn ? BoardMaterials_SO.Piece_p1Color.color : BoardMaterials_SO.Piece_p2Color.color;
    }


}
