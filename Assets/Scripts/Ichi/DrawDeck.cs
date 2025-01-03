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

    public void DrawCard(Transform deckTransform)
    {
        Card card = Instantiate(Card_Prefab, deckTransform);

        int index = Random.Range(0, TheDeck.Count);
        string attribute = TheDeck[index];

        card.SetColor(attribute.Substring(0,attribute.Length-1));
        card.SetValue(attribute[attribute.Length-1].ToString());

        TheDeck.Remove(attribute);

        ArrangeDeck(deckTransform);
    }

    private void ArrangeDeck(Transform deckTransform)
    {
        int cardCount = deckTransform.childCount;

        if (cardCount == 0)
            return;

        else if (cardCount == 1)
            deckTransform.GetChild(0).SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        else
        {
            // Total range and spacing calculations
            float topRange = 3f * cardCount;
            float bottomRange = -topRange;

            float totalRange = topRange - bottomRange;
            float spacing = totalRange / (cardCount - 1);
            float z = 0;

            int i=-1;
            foreach (Transform card in deckTransform)
            {
                // Calculate position for the current object within the local space of the deck
                float local_x = bottomRange + (++i * spacing);
                Vector3 localPosition = new Vector3(local_x, 0, z);

                card.SetLocalPositionAndRotation(localPosition, Quaternion.identity);

                z -= 0.1f;// Increment so card wont overlap on each other
            }
        }
        
    }
}
