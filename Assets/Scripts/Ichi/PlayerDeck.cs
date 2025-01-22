using System.Collections.Generic;
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    [SerializeField] CardSO Card_SO;

    public void SortDeck()
    {
        int cardCount = this.transform.childCount;

        if (cardCount == 0)
            return;

        else if (cardCount == 1)
            this.transform.GetChild(0).SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        else
        {
            Transform deckTransform = this.transform;

            Transform[] children = new Transform[deckTransform.childCount];
            for (int i=0; i<deckTransform.childCount; ++i)
                children[i] = deckTransform.GetChild(i);

            // Sort the array based on the child names
            System.Array.Sort(children, (x, y) => 
            {
                // Get Card components
                Card cardX = x.GetComponent<Card>();
                Card cardY = y.GetComponent<Card>();

                // Primary sort: if color are different, sort by color
                int colorComparison = string.Compare(cardX.Material.name, cardY.Material.name);
                if (colorComparison != 0)
                    return colorComparison; // If colors are different, sort by color
                
                // Secondary sort: By value
                string valueX = cardX.Value;
                string valueY = cardY.Value;

                // Special cards come first (those with more than one character in their value)
                bool isSpecialX = valueX.Length > 1;
                bool isSpecialY = valueY.Length > 1;

                if (isSpecialX && !isSpecialY)
                    return -1; // Special card X comes before non-special card Y
                if (!isSpecialX && isSpecialY)
                    return 1; // Non-special card X comes after special card Y

                // If both are special or both are not, sort numeric values in descending order
                if (int.TryParse(valueX, out int numericX) && int.TryParse(valueY, out int numericY))
                    return numericY.CompareTo(numericX); // Descending order

                // If the values are not numeric (e.g., "Skip" vs "Draw2"), use default string comparison
                return string.Compare(valueX, valueY, System.StringComparison.Ordinal);
            });
            // Reorder the children in the hierarchy
            for (int i = 0; i < children.Length; i++)
                children[i].SetSiblingIndex(i);

            ArrangeDeck(cardCount);
        }
    }
    public void ArrangeDeck(int cardCount)
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

            z -= 0.1f; // Increment so card wont overlap on each other
        }
    }
}
