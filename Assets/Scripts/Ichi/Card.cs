using UnityEngine;
using UnityEngine.Events;

public class Card : MonoBehaviour
{
    private const int CARD_LIFT = 20;

    [SerializeField] private CardSO Card_SO;
    [SerializeField] private SpriteRenderer[] CardSprites;
    [SerializeField] private BoxCollider BoxCollider;

    public string Value;
    public Material Material;
    private Transform PlayPile;

    public event System.Action<Card> OnPlayedCard;

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

        ManueverCard(upDirection: false);
        SetCollider(false);
        SetCardOnPlayPile();

        OnPlayedCard?.Invoke(this);     
    }
    private bool CanPlayCard()
    {
        Card topCard = PlayPile.transform.GetChild(PlayPile.transform.childCount-1).GetComponent<Card>();
        return !((topCard.Material.name != this.Material.name) && (topCard.Value != this.Value));
    }
    private void SetCardOnPlayPile()
    {
        this.transform.SetParent(PlayPile);
        this.transform.rotation = Quaternion.Euler(90f, 0, Random.Range(0, 361));

        PlayPile.GetComponent<PlayPile>().RearrangePlayPile();
    }


    public void SetColor(string color)
    {
        Material mat = Card_SO.GetMaterial(color);

        this.GetComponent<MeshRenderer>().material = mat;
        this.Material = mat;
    }
    public void SetValue(string val)
    {
        this.Value = val;

        Sprite sprite = Card_SO.GetSprite(val);
        foreach (SpriteRenderer spriteRenderer in CardSprites)
            spriteRenderer.sprite = sprite;
    }
    public void SetCollider(bool enable)
    {
        this.BoxCollider.enabled = enable;
    }
}
