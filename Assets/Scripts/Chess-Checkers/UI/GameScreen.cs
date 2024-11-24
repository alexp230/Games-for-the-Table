using TMPro;
using UnityEngine;

public class GameScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI PlayerTurnText;
    [SerializeField] private BoardMaterials BoardMaterials_SO;


    public void SetPlayerTurnText(bool isP1Turn)
    {
        PlayerTurnText.text = isP1Turn ? "Light" : "Dark";
        PlayerTurnText.color = isP1Turn ? BoardMaterials_SO.Piece_p1Color.color : BoardMaterials_SO.Piece_p2Color.color;
    }


}
