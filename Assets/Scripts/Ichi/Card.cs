using UnityEngine;
using TMPro;

public class Card : MonoBehaviour
{
    [SerializeField] private CardSO Card_SO;

    [SerializeField] private TextMeshProUGUI MiddleText;
    [SerializeField] private TextMeshProUGUI TopLeftText;
    [SerializeField] private TextMeshProUGUI BottomRightText;
    [SerializeField] private BoxCollider BoxCollider;

    private char Value;
    private const int CARD_LIFT = 20;

    void Start()
    {
        // this.SetColor();
        // this.SetValue();
    }

    void OnMouseEnter()
    {
        Vector3 pos = this.transform.position;
        this.transform.position = new Vector3(pos.x, pos.y+CARD_LIFT, pos.z);

        Vector3 boxPos = this.BoxCollider.size;
        this.BoxCollider.size = new Vector3(boxPos.x, boxPos.y*2, boxPos.z);
    }

    void OnMouseExit()
    {
        Vector3 pos = this.transform.position;
        this.transform.position = new Vector3(pos.x, pos.y-CARD_LIFT, pos.z);

        Vector3 boxPos = this.BoxCollider.size;
        this.BoxCollider.size = new Vector3(boxPos.x, boxPos.y/2, boxPos.z);
    }

    
    public void SetColor(string color = "")
    {
        if (color == "")
        {
            Material[] mats = new Material[4] {Card_SO.ColorRed, Card_SO.ColorYellow, Card_SO.ColorGreen, Card_SO.ColorCyan};
            this.GetComponent<MeshRenderer>().material = mats[Random.Range(0, mats.Length)];
            return;
        }
        switch (color)
        {
            case "red": this.GetComponent<MeshRenderer>().material = Card_SO.ColorRed; break;
            case "green": this.GetComponent<MeshRenderer>().material = Card_SO.ColorGreen; break;
            case "yellow": this.GetComponent<MeshRenderer>().material = Card_SO.ColorYellow; break;
            case "cyan": this.GetComponent<MeshRenderer>().material = Card_SO.ColorCyan; break;
        }        
    }
    public void SetValue(string val = "z")
    {
        if (val != "z")
        {
            this.Value = val[0];
            MiddleText.text = val;
            TopLeftText.text = val;
            BottomRightText.text = val;
        }
        else
        {
            string value = Random.Range(1, 10).ToString();
            this.Value = value[0];
            MiddleText.text = value;
            TopLeftText.text = value;
            BottomRightText.text = value;
        }
        
        
    }
}
