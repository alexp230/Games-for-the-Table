using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Objects/CardSO")]
public class CardSO : ScriptableObject
{
    public Material ColorRed;
    public Material ColorYellow;
    public Material ColorCyan;
    public Material ColorGreen;
    public static string[] COLORS = new string[] {"Red", "Yellow", "Cyan", "Green"};

    public Material GetMaterial(string color)
    {
        switch (color)
        {
            case "Red": return ColorRed;
            case "Yellow": return ColorYellow;
            case "Cyan": return ColorCyan;
            case "Green": return ColorGreen;

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
    public static string[] TYPES = new string[] { "0","1","2","3","4","5","6","7","8","9","cancel","reverse","plus2","plus4",
        "plus6" };

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
            
            default: return null;
        }
    }
}
