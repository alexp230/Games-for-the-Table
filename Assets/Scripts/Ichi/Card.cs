using UnityEngine;

public class Card : MonoBehaviour
{
    private const int CARD_LIFT = 20;

    [SerializeField] private CardSO Card_SO;
    [SerializeField] private SpriteRenderer[] CardSprites;
    [SerializeField] private BoxCollider BoxCollider;
    private Transform PlayPile;
    private Ichi Ichi_S;

    public string Value;
    public Material Material;
    public bool IsSpecialCard;

    public event System.Action<Card> OnPlayedCard;

    void Start()
    {
        PlayPile = GameObject.Find("PlayPile").transform;
        Ichi_S = GameObject.Find("Ichi").GetComponent<Ichi>();
    }
    public void InitializeCard(string type, string color)
    {
        SetSpecialCard(type);
        SetColor(color);
        SetValue(type);
    }
    private void SetSpecialCard(string val)
    {
        this.IsSpecialCard = Card_SO.IsSpecialCard(Card_SO.GetSprite(val));
    }
    public void SetColor(string color)
    {
        Material mat = Card_SO.GetMaterial(color);

        this.GetComponent<MeshRenderer>().material = mat;
        this.Material = mat;
    }
    public void SetColor(Material mat)
    {
        this.GetComponent<MeshRenderer>().material = mat;
        this.Material = mat;
    }
    private void SetValue(string val)
    {
        this.Value = val;

        Sprite sprite = Card_SO.GetSprite(val);
        foreach (SpriteRenderer spriteRenderer in CardSprites)
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = this.IsSpecialCard ? Color.white : Color.black;
        }
    }

    void OnMouseEnter()
    {
        if (CanPlayCard())
            ManueverCard(upDirection: true);
    }

    void OnMouseExit()
    {
        if (CanPlayCard())
            ManueverCard(upDirection: false);
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
        if (!this.BoxCollider.enabled)
            return false;
        if (GameObject.Find($"PlayerDeck{Ichi_S.DeckCount}").transform != this.transform.parent)
            return false;

        Card topCard = PlayPile.transform.GetChild(PlayPile.transform.childCount-1).GetComponent<Card>();
        if (topCard.Material.name == this.Material.name)
            return true;
        if (topCard.Value == this.Value)
            return true;
        if (this.IsSpecialCard)
            return true;
        return false;
    }
    private void ManueverCard(bool upDirection)
    {
        Vector3 pos = this.transform.localPosition;
        this.transform.localPosition = new Vector3(pos.x, pos.y + (upDirection ? CARD_LIFT : -CARD_LIFT), pos.z);

        Vector3 boxPos = this.BoxCollider.size;
        this.BoxCollider.size = new Vector3(boxPos.x, boxPos.y * (upDirection ? 2f : 0.5f), boxPos.z);
    }
    private void SetCardOnPlayPile()
    {
        this.transform.SetParent(PlayPile);
        this.transform.rotation = Quaternion.Euler(90f, 0, Random.Range(0, 361));

        PlayPile.GetComponent<PlayPile>().RearrangePlayPile();
    }


    public void SetCollider(bool enable)
    {
        this.BoxCollider.enabled = enable;
    }
}
