using TMPro;
using UnityEngine;

public class GameScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI PlayerTurnText;
    [SerializeField] private BoardMaterials BoardMaterials_SO;

    private bool HasShowValidMovesValueBeenOn;

    void OnEnable() 
    {
        HasShowValidMovesValueBeenOn = BoardMaterials.ShowValidMoves;
    }

    public void SetPlayerTurnText(bool isP1Turn)
    {
        PlayerTurnText.text = isP1Turn ? "Light" : "Dark";
        PlayerTurnText.color = isP1Turn ? BoardMaterials_SO.Piece_p1Color.color : BoardMaterials_SO.Piece_p2Color.color;
    }

    public void SetHasShowValidMovesValueBeenOn()
    {
        if (BoardMaterials.ShowValidMoves)
            HasShowValidMovesValueBeenOn = true;
    }

    public void SetShowValidMoveAchievement()
    {
        if (ChessBoard.DidThisPlayerMove() || HasShowValidMovesValueBeenOn)
            return;
        
        switch (BoardMaterials.GameType)
        {
            case BoardMaterials.CHECKERS_GAME: SteamAchievements.UnlockAchievement("NEW_ACHIEVEMENT_1_6"); break;
            case BoardMaterials.CHESS_GAME: SteamAchievements.UnlockAchievement("NEW_ACHIEVEMENT_1_7"); break;
            // case BoardMaterials.CHECKERS_GAME: SteamAchievements.UnlockAchievement("NEW_ACHIEVEMENT_1_1"); break;
        }
    }


}
