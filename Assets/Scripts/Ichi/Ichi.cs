using UnityEngine;

public class Ichi : MonoBehaviour
{
    [SerializeField] private Card Card_Prefab;
    private int numberOfCards = 7; // Cards per player deck

    private int numberOfObjects = 3; // Number of objects to spawn
    private float radius = 90f; // Radius of the circle

    void Start()
    {
        SpawnObjects();
        SetCamera();
    }

    private void SpawnObjects()
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
        }
    }

    private void SpawnCards(Transform deckTransform)
    {
        // Total range and spacing calculations
        float topRange = 3.5f * numberOfCards;
        float bottomRange = -topRange;

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
            Instantiate(Card_Prefab, worldPosition, deckTransform.rotation);

            // Increment for slight horizontal offset
            z -= 0.1f;
        }
    }

    private void SetCamera()
    {
        string player = "PlayerDeck0";
        GameObject playerDeck = GameObject.Find(player);

        print(player);
        
        // Set the camera's position relative to the deck
        Vector3 offset = playerDeck.transform.forward * -100f + playerDeck.transform.up * 70f; // Move back and up
        Camera.main.transform.position = playerDeck.transform.position + offset;

        // Rotate the camera to look at the deck
        Camera.main.transform.LookAt(playerDeck.transform);

        // Camera.main.transform.Rotate(10f, 0f, 0f, Space.Self);
    }

}
