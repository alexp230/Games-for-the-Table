using UnityEngine;

public class Ichi : MonoBehaviour
{
    [SerializeField] private Card Card_Prefab;
    private int numberOfCards = 3; // Cards per player deck

    private int numberOfObjects = 3; // Number of objects to spawn
    private float radius = 45f; // Radius of the circle

    void Start()
    {
        SpawnObjects();
    }

    void SpawnObjects()
    {
        for (int i=0; i<numberOfObjects; ++i)
        {
            // Calculate angle in radians
            float angle = i*Mathf.PI*2f / numberOfObjects;

            // Calculate position on the circle
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            // Create position relative to the central object
            Vector3 position = new Vector3(x, 0, z) + transform.position;

            // Calculate the direction vector from the central object to the position
            Vector3 direction = (position - transform.position).normalized;
            // Calculate rotation to face away from the central object
            Quaternion rotation = Quaternion.LookRotation(-direction);

            // Instantiate the object
            // Instantiate(Card_Prefab, position, rotation);
            GameObject playerDeck = new GameObject($"PlayerDeck{i}");
            playerDeck.transform.SetPositionAndRotation(position, rotation);

            SpawnCards(playerDeck.transform);

            // Spawn cards in an arc for this player deck
            // Card[] cards = SpawnCardsInArc(playerDeck.transform);
            // ArrangeCardsInHand(playerDeck.transform, cards, cardArcAngle, cardArcRadius);
        }
    }

    private void SpawnCards(Transform deckTransform)
    {
        // Total range and spacing calculation
        int topRange = 10;
        int bottomRange = -10;

        float totalRange = topRange - bottomRange;
        float spacing = totalRange / (numberOfCards - 1);
        float z = 0;

        for (int i = 0; i < numberOfCards; ++i)
        {
            // Calculate position for the current object within the local space of the deck
            float local_x = bottomRange + i * spacing;
            Vector3 localPosition = new Vector3(local_x, 0, z);

            // Convert local position to world position relative to the deck
            Vector3 worldPosition = deckTransform.TransformPoint(localPosition);

            // Instantiate card at the correct world position and rotation
            Card card = Instantiate(Card_Prefab, worldPosition, deckTransform.rotation);
            card.SetColor();

            // Increment for slight horizontal offset
            z -= 0.1f;
        }
    }

}
