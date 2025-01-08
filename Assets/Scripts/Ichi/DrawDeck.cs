using System.Collections.Generic;
using UnityEngine;

public class DrawDeck : MonoBehaviour
{
    [SerializeField] private Ichi Ichi_S;
    [SerializeField] private Card Card_Prefab;
    [SerializeField] private List<string> TheDeck = new List<string>();
    
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
            foreach (string number in numbers)
                TheDeck.Add(color+number);
    }

    private void DrawToPlayPile()
    {
        Transform playPile = GameObject.Find("PlayPile").transform;

        Card card = Instantiate(Card_Prefab, playPile);

        int index = Random.Range(0, TheDeck.Count);
        string attribute = TheDeck[index];

        card.SetColor(attribute.Substring(0,attribute.Length-1));
        card.SetValue(attribute[attribute.Length-1].ToString());

        card.transform.SetParent(playPile);

        card.transform.localPosition = Vector3.zero;
        card.transform.rotation = Quaternion.Euler(90f, 0, Random.Range(0, 361));

        TheDeck.Remove(attribute);
    }

    public void DrawCard(Transform deckTransform)
    {
        Card card = Instantiate(Card_Prefab, deckTransform);

        int index = Random.Range(0, TheDeck.Count);
        string attribute = TheDeck[index];

        card.SetColor(attribute.Substring(0,attribute.Length-1));
        card.SetValue(attribute[attribute.Length-1].ToString());

        TheDeck.Remove(attribute);

        deckTransform.GetComponent<PlayerDeck>().ArrangeDeck();
    }

}
