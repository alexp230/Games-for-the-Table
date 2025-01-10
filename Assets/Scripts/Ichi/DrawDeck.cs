using System.Collections.Generic;
using UnityEngine;

public class DrawDeck : MonoBehaviour
{
    [SerializeField] private Ichi Ichi_S;
    [SerializeField] private Card Card_Prefab;

    public List<Card> TheDeck = new List<Card>();
    
    void Awake()
    {
        CreateDeck();
        DrawToPlayPile();
    }

    void OnMouseDown()
    {
        Transform deck = GameObject.Find($"PlayerDeck{Ichi_S.DeckCount}").transform;
        DrawCard(deck);
    }

    private void CreateDeck()
    {
        foreach (string color in new string[] { "red", "green", "yellow", "cyan"})
            foreach (char type in new char[] { '0','1','2','3','4','5','6','7','8','9','c','r' })
                CreateCard(Card_Prefab, color, type);

        void CreateCard(Card cardPrefab, string color, char type)
        {
            Card card = Instantiate(cardPrefab, this.transform);
            card.SetColor(color);
            card.SetValue(type);

            TheDeck.Add(card);
        }
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

        deckTransform.GetComponent<PlayerDeck>().ArrangeDeck();
    }

    public void AddToDeck(Card card)
    {
        TheDeck.Add(card);
    }

}
