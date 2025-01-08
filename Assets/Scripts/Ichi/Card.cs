using UnityEngine;
using TMPro;

public class Card : MonoBehaviour
{
    private const int CARD_LIFT = 20;

    [SerializeField] private CardSO Card_SO;

    [SerializeField] private TextMeshProUGUI MiddleText;
    [SerializeField] private TextMeshProUGUI TopLeftText;
    [SerializeField] private TextMeshProUGUI BottomRightText;
    [SerializeField] private BoxCollider BoxCollider;

    private char Value;
    private Transform PlayPile;


    void Start()
    {
        PlayPile = GameObject.Find("PlayPile").transform;
    }

    void OnMouseEnter()
    {
        Vector3 pos = this.transform.localPosition;
        this.transform.localPosition = new Vector3(pos.x, pos.y+CARD_LIFT, pos.z);

        Vector3 boxPos = this.BoxCollider.size;
        this.BoxCollider.size = new Vector3(boxPos.x, boxPos.y*2, boxPos.z);
    }

    void OnMouseExit()
    {
        Vector3 pos = this.transform.localPosition;
        this.transform.localPosition = new Vector3(pos.x, pos.y-CARD_LIFT, pos.z);

        Vector3 boxPos = this.BoxCollider.size;
        this.BoxCollider.size = new Vector3(boxPos.x, boxPos.y/2, boxPos.z);
    }

    void OnMouseDown()
    {
        Card topCard = PlayPile.transform.GetChild(PlayPile.transform.childCount-1).GetComponent<Card>();

        if ((topCard.GetComponent<MeshRenderer>().material.name != this.GetComponent<MeshRenderer>().material.name) && (topCard.Value != this.Value))
            return;
            
        PlayerDeck playerDeck = this.transform.parent.GetComponent<PlayerDeck>();

        this.transform.SetParent(PlayPile);

        this.transform.localPosition = new Vector3(0, (PlayPile.childCount*0.2f)+CARD_LIFT, 0);
        this.transform.rotation = Quaternion.Euler(90f, 0, Random.Range(0, 361));

        playerDeck.ArrangeDeck();
    }

    
    public void SetColor(string color = "")
    {
        Material[] mats = new Material[4] {Card_SO.ColorRed, Card_SO.ColorYellow, Card_SO.ColorGreen, Card_SO.ColorCyan};

        switch (color)
        {
            case "red": this.GetComponent<MeshRenderer>().material = mats[0]; break;
            case "yellow": this.GetComponent<MeshRenderer>().material = mats[1]; break;
            case "green": this.GetComponent<MeshRenderer>().material = mats[2]; break;
            case "cyan": this.GetComponent<MeshRenderer>().material = mats[3]; break;
            default:  this.GetComponent<MeshRenderer>().material = mats[Random.Range(0, mats.Length)]; break;
        }        
    }
    public void SetValue(string val = "z")
    {
        if (val == "z")
            val = Random.Range(1, 10).ToString();

        this.Value = val[0];
        MiddleText.text = val;
        TopLeftText.text = val;
        BottomRightText.text = val;
    }
}
