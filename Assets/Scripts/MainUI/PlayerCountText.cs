using TMPro;
using UnityEngine;

public class PlayerCountText : MonoBehaviour
{    
    [SerializeField] private TextMeshProUGUI MaxPlayerCount;
    [SerializeField] private TMP_Dropdown GameModeSelection;

    public void ChangeMaxPlayerCount()
    {
        MaxPlayerCount.text = "MaxPlayers: ";

        switch(GameModeSelection.options[GameModeSelection.value].text)
        {
            case "CHECKERS": MaxPlayerCount.text+="2"; break;
            case "CHESS": MaxPlayerCount.text+="4"; break;
            case "COMBINATION": MaxPlayerCount.text+="6"; break;
        }
    }
}
