using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    
    public void ArrangeDeck()
    {
        int cardCount = this.transform.childCount;

        if (cardCount == 0)
            return;

        else if (cardCount == 1)
            this.transform.GetChild(0).SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        else
        {
            // Total range and spacing calculations
            float topRange = 3f * cardCount;
            float bottomRange = -topRange;

            float totalRange = topRange - bottomRange;
            float spacing = totalRange / (cardCount - 1);
            float z = 0;

            int i=-1;
            foreach (Transform card in this.transform)
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
