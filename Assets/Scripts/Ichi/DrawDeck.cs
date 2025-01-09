using System.Collections.Generic;
using UnityEngine;

public class DrawDeck : MonoBehaviour
{
    [SerializeField] private Ichi Ichi_S;
    [SerializeField] private Card Card_Prefab;

    public List<Card> ELDeck = new List<Card>();
    
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
        string[] colors = new string[] { "red", "green", "yellow", "cyan"};
        string[] numbers = new string[] { "0","1","2","3","4","5","6","7","8","9" };

        foreach (string color in colors)
        {
            foreach (string number in numbers)
            {
                Card card = Instantiate(Card_Prefab);

                card.SetColor(color);
                card.SetValue(number);

                ELDeck.Add(card);
            }
        }
    }

    private void DrawToPlayPile()
    {
        Card topCard = ELDeck[Random.Range(0, ELDeck.Count)];

        Transform playPile = GameObject.Find("PlayPile").transform;
        topCard.transform.SetParent(playPile);

        topCard.transform.localPosition = Vector3.zero;
        topCard.transform.rotation = Quaternion.Euler(90f, 0, Random.Range(0, 361));

        ELDeck.Remove(topCard);
    }

    public void DrawCard(Transform deckTransform)
    {
        Card card = ELDeck[Random.Range(0, ELDeck.Count)];
        card.transform.SetParent(deckTransform);

        ELDeck.Remove(card);

        deckTransform.GetComponent<PlayerDeck>().ArrangeDeck();
    }

}
