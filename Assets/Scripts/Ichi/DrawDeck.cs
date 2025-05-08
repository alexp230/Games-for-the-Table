using System.Collections.Generic;
using UnityEngine;

public class DrawDeck : MonoBehaviour
{
    [SerializeField] private Ichi Ichi_S;
    [SerializeField] private Card Card_Prefab;

    public List<Card> TheDeck = new List<Card>();

    public event System.Action OnDrawCard;
    
    void Awake()
    {
        CreateDeck();
        DrawToPlayPile();
    }

    void OnMouseDown()
    {
        Transform deck = GameObject.Find($"PlayerDeck{Ichi_S.DeckCount}").transform;
        DrawCard(deck);

        OnDrawCard?.Invoke();
    }

    private void CreateDeck()
    {
        foreach (string color in CardSO.COLORS)
            foreach (string type in CardSO.TYPES)
                CreateCard(Card_Prefab, color, type);
                
        foreach (string specialType in CardSO.SPECIAL_TYPES)
            CreateCard(Card_Prefab, "Black", specialType);

        void CreateCard(Card cardPrefab, string color, string type)
        {
            Card card = Instantiate(cardPrefab, this.transform);
            card.InitializeCard(type, color);

            TheDeck.Add(card);
        }
        print(TheDeck.Count);
    }

    private void DrawToPlayPile()
    {
        Card topCard = TheDeck[Random.Range(0, TheDeck.Count)];

        Transform playPile = GameObject.Find("PlayPile").transform;
        topCard.transform.SetParent(playPile);

        topCard.transform.localPosition = Vector3.zero;
        topCard.transform.rotation = Quaternion.Euler(90f, 0, Random.Range(0, 361));

        TheDeck.Remove(topCard);
    }

    public void DrawCard(Transform deckTransform)
    {
        Card card = TheDeck[Random.Range(0, TheDeck.Count)];
        card.transform.SetParent(deckTransform);
        card.SetCollider(true);

        TheDeck.Remove(card);
        deckTransform.GetComponent<PlayerDeck>().SortDeck();
    }

    public void AddToDeck(Card card)
    {
        TheDeck.Add(card);
    }

}
