using System.Collections.Generic;
using UnityEngine;

public class Ichi : MonoBehaviour
{
    [SerializeField] private Card Card_Prefab;

    public List<string> Deck = new List<string>();

    private int NumberOfDecks = 3; // Number of objects to spawn
    private int numberOfCards = 7; // Cards per player deck

    private int DeckCount = 0;

    void Start()
    {
        CreateDeck();
        SpawnDecks();
        SetCamera();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (++DeckCount == NumberOfDecks)
                DeckCount = 0;
            SetCamera();            
        }

        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (--DeckCount == 0)
                DeckCount = NumberOfDecks-1;
            SetCamera();
        }
    }

    private void CreateDeck()
    {
        string[] colors = new string[] { "red", "green", "yellow", "cyan"};
        string[] numbers = new string[] { "0","1","2","3","4","5","6","7","8","9" };
        foreach (string color in colors)
        {
            foreach (string number in numbers)
            {
                Deck.Add(color+number);
            }
        }
    }
    

    private void SpawnDecks()
    {
        float Radius = 90f; // Radius of the circle

        for (int i=0; i<NumberOfDecks; ++i)
        {
            // Calculate angle in radians
            float angle = i*Mathf.PI*2f / NumberOfDecks;

            // Calculate position on the circle
            float x = Mathf.Cos(angle) * Radius;
            float z = Mathf.Sin(angle) * Radius;

            // Create position relative to the central object
            Vector3 position = new Vector3(x, 0, z) + transform.position;

            // Calculate the direction vector from the central object to the position
            Vector3 direction = (position - transform.position).normalized;
            // Calculate rotation to face away from the central object
            Quaternion rotation = Quaternion.LookRotation(-direction);

            // Instantiate the deck
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
            int index = Random.Range(0, Deck.Count);
            Card card = Instantiate(Card_Prefab, worldPosition, deckTransform.rotation);
            string attribute = Deck[index];
            card.SetColor(attribute.Substring(0,attribute.Length-1));
            card.SetValue(attribute[attribute.Length-1].ToString());
            Deck.Remove(attribute);

            // Increment for slight horizontal offset
            z -= 0.1f;
        }
    }

    private void SetCamera()
    {
        string player = $"PlayerDeck{DeckCount}";
        GameObject playerDeck = GameObject.Find(player);

        print(player);
        
        // Set the camera's position relative to the deck
        Vector3 offset = playerDeck.transform.forward * -100f + playerDeck.transform.up * 70f; // Move back and up
        Camera.main.transform.position = playerDeck.transform.position + offset;

        // Rotate the camera to look at the deck
        Camera.main.transform.LookAt(playerDeck.transform);
        Camera.main.transform.Rotate(-12f, 0f, 0f, Space.Self); // rotate camera a little up
    }

}
