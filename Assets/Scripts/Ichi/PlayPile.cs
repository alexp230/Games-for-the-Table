using UnityEngine;

public class PlayPile : MonoBehaviour
{
    [SerializeField] private DrawDeck DrawDeck_S;



    public void RearrangePlayPile()
    {
        int cardCount = this.transform.childCount;

        if (cardCount >= 6)
        {
            Card card = this.transform.GetChild(0).GetComponent<Card>();

            card.transform.SetParent(DrawDeck_S.transform);
            card.transform.position = Vector3.zero;
            card.transform.rotation = Quaternion.identity;

            DrawDeck_S.AddToDeck(card);
        }

        RearrangePile();
    }

    private void RearrangePile()
    {
        int i=-1;
        foreach (Transform card in this.transform)
        {
            card.position = Vector3.zero;
            card.localPosition = new Vector3(0, ++i*0.25f, 0);
        }
    }

}
