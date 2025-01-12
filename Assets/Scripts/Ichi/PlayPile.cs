using UnityEngine;

public class PlayPile : MonoBehaviour
{
    [SerializeField] private DrawDeck DrawDeck_S;


    public void RearrangePlayPile()
    {
        if (this.transform.childCount >= 6)
        {
            Card card = this.transform.GetChild(0).GetComponent<Card>();

            if (card.IsSpecialCard)
                card.SetColor("Black");
            card.transform.SetParent(DrawDeck_S.transform);
            card.transform.position = Vector3.zero;
            card.transform.rotation = Quaternion.identity;

            DrawDeck_S.AddToDeck(card);
        }

        ShuffleDeck();
    }

    private void ShuffleDeck()
    {
        int i=0;
        foreach (Transform card in this.transform)
        {
            card.position = Vector3.zero;
            card.localPosition = new Vector3(0, ++i*0.25f, 0);
        }
    }

}
