using UnityEngine;

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Objects/CardSO")]
public class CardSO : ScriptableObject
{
    public Material ColorRed;
    public Material ColorYellow;
    public Material ColorCyan;
    public Material ColorGreen;
    public Material ColorWhite;
    public Material ColorBlack;
    public static string[] COLORS = new string[] {"Red", "Yellow", "Cyan", "Green"};

    public Material GetMaterial(string color)
    {
        switch (color)
        {
            case "Red": return ColorRed;
            case "Yellow": return ColorYellow;
            case "Cyan": return ColorCyan;
            case "Green": return ColorGreen;
            case "White": return ColorWhite;
            case "Black": return ColorBlack;

            default: return null;
        }
    }

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
    public Sprite Plus2;
    public Sprite Plus4;
    public Sprite Plus6;
    public Sprite Wild;
    public Sprite Wild4;
    public Sprite Shift;
    public Sprite WildErase;
    public static string[] TYPES = new string[] {"0","1","2","3","4","5","6","7","8","9","cancel","reverse","plus2","plus4",
        "plus6" };
    public static string[] SPECIAL_TYPES = new string[] { "wild", "wild4", "shift", "wildErase" };

    public Sprite GetSprite(string type)
    {
        switch (type)
        {
            case "0": return Number0;
            case "1": return Number1;
            case "2": return Number2;
            case "3": return Number3;
            case "4": return Number4;
            case "5": return Number5;
            case "6": return Number6;
            case "7": return Number7;
            case "8": return Number8;
            case "9": return Number9;
            case "cancel": return Cancel;
            case "reverse": return Reverse;
            case "plus2": return Plus2;
            case "plus4": return Plus4;
            case "plus6": return Plus6;
            case "wild": return Wild;
            case "wild4": return Wild4;
            case "shift": return Shift;
            case "wildErase": return WildErase;
            
            default: return null;
        }
    }

    public bool IsSpecialCard(Sprite sprite)
    {
        foreach (string cardName in SPECIAL_TYPES)
            if (cardName == sprite.name)
                return true;
        return false;
    }
}
