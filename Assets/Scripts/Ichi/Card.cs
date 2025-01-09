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

    public char CardValue;
    public Material CardMaterial;
    private Transform PlayPile;


    void Start()
    {
        PlayPile = GameObject.Find("PlayPile").transform;
    }

    void OnMouseEnter()
    {
        ManueverCard(upDirection: true);
    }

    void OnMouseExit()
    {
        if (this.BoxCollider.enabled)
            ManueverCard(upDirection: false);
    }

    private void ManueverCard(bool upDirection)
    {
        Vector3 pos = this.transform.localPosition;
        this.transform.localPosition = new Vector3(pos.x, pos.y + (upDirection ? CARD_LIFT : -CARD_LIFT), pos.z);

        Vector3 boxPos = this.BoxCollider.size;
        this.BoxCollider.size = new Vector3(boxPos.x, boxPos.y * (upDirection ? 2f : 0.5f), boxPos.z);
    }

    void OnMouseDown()
    {
        if (!CanPlayCard())
            return;

        PlayerDeck playerDeck = this.transform.parent.GetComponent<PlayerDeck>();

        SetCardOnPlayPile();

        playerDeck.ArrangeDeck();
    }

    private bool CanPlayCard()
    {
        Card topCard = PlayPile.transform.GetChild(PlayPile.transform.childCount-1).GetComponent<Card>();
        return !((topCard.CardMaterial.name != this.CardMaterial.name) && (topCard.CardValue != this.CardValue));
    }
    private void SetCardOnPlayPile()
    {        
        this.transform.SetParent(PlayPile);
        this.transform.rotation = Quaternion.Euler(90f, 0, Random.Range(0, 361));

        SetCollider(false);

        PlayPile.GetComponent<PlayPile>().RearrangePlayPile();
    }


    public void SetColor(string color)
    {
        Material[] mats = new Material[4] {Card_SO.ColorRed, Card_SO.ColorYellow, Card_SO.ColorGreen, Card_SO.ColorCyan};
        Material mat;

        switch (color)
        {
            case "red": mat = mats[0]; break;
            case "yellow": mat = mats[1]; break;
            case "green": mat = mats[2]; break;
            case "cyan": mat = mats[3]; break;
            default:  mat = mats[Random.Range(0, mats.Length)]; break;
        }

        this.GetComponent<MeshRenderer>().material = mat;
        CardMaterial = mat;
    }
    public void SetValue(string val)
    {
        this.CardValue = val[0];

        if (val == "s" || val == "r")
            return;

        MiddleText.text = val;
        TopLeftText.text = val;
        BottomRightText.text = val;
    }
    public void SetCollider(bool enable)
    {
        this.BoxCollider.enabled = enable;
    }
}
