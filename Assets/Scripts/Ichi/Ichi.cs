using UnityEngine;

public class Ichi : MonoBehaviour
{
    [SerializeField] private DrawDeck DrawDeck_S;
    [SerializeField] private PlayerDeck PlayerDeck_P;

    private int NumberOfDecks = 4;
    private int NumberOfCards = 7;

    public int DeckCount = 0;
    public int PlayerTurn = 0;

    void Start()
    {
        SubScribeToCardEvents();
        SpawnDecks();
        SetCamera();
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
            Vector3 position = new Vector3(x, 10, z) + transform.position;

            // Calculate the direction vector from the central object to the position
            Vector3 direction = (position - transform.position).normalized;

            // Calculate rotation to face away from the central object
            Quaternion rotation = Quaternion.LookRotation(-direction);

            // Instantiate the deck
            PlayerDeck playerDeck = Instantiate(PlayerDeck_P);
            playerDeck.transform.SetPositionAndRotation(position, rotation);
            playerDeck.name = $"PlayerDeck{i}";

            for (int j=0; j<NumberOfCards; ++j)
                DrawDeck_S.DrawCard(playerDeck.transform);
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
    private void SubScribeToCardEvents()
    {
        foreach (Card card in DrawDeck_S.TheDeck)
            card.OnPlayedCard += OnCardPlayed;
    }

    void OnDisable()
    {
        UnSubScribeToCardEvents();
    }
    private void UnSubScribeToCardEvents()
    {
        foreach (Card card in DrawDeck_S.TheDeck)
            card.OnPlayedCard -= OnCardPlayed;
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
            if (--DeckCount == -1)
                DeckCount = NumberOfDecks-1;
            SetCamera();
        }
    }
    

    public void ReArrangeDeck()
    {
        GameObject.Find($"PlayerDeck{DeckCount}").GetComponent<PlayerDeck>().ArrangeDeck();
    }

    private void OnCardPlayed(Card playedCard)
    {
        ReArrangeDeck();
    }

}
