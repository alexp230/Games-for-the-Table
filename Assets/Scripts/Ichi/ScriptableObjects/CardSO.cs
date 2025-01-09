using UnityEngine;

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Objects/CardSO")]
public class CardSO : ScriptableObject
{
    public Material ColorRed;
    public Material ColorYellow;
    public Material ColorCyan;
    public Material ColorGreen;

    public Sprite Number0;
    public Sprite Number1;
    public Sprite Number2;
    public Sprite Number3;
    public Sprite Number4;
    public Sprite Number5;
    public Sprite Number6;
    public Sprite Number7;
    public Sprite Number8;
    public Sprite Number9;
    public Sprite Cancel;
    public Sprite Reverse;

    public Sprite GetSprite(char type)
    {
        switch (type)
        {
            case '0': return Number0;
            case '1': return Number1;
            case '2': return Number2;
            case '3': return Number3;
            case '4': return Number4;
            case '5': return Number5;
            case '6': return Number6;
            case '7': return Number7;
            case '8': return Number8;
            case '9': return Number9;
            case 'c': return Cancel;
            case 'r': return Reverse;
            
            default: return Number0;
        }
    }


}
