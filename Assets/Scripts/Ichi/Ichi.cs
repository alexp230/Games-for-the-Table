using System.Collections;
using UnityEngine;

public class Ichi : MonoBehaviour
{
    [SerializeField] private DrawDeck DrawDeck_S;
    [SerializeField] private PlayerDeck PlayerDeck_P;

    private int NumberOfDecks = 4;
    private int NumberOfCards = 7;

    public int DeckCount = 0;
    private bool ReverseMode = false;

    void Start()
    {
        SpawnDecks();
        SetCamera();
        SubScribeToEvents();
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

        this.transform.LookAt(GameObject.Find($"PlayerDeck{DeckCount}").transform);
        this.transform.Rotate(0f, 180f, 0f);
    }
    private void SubScribeToEvents()
    {
        DrawDeck_S.OnDrawCard += OnDrawCard;
        foreach (GameObject card in GameObject.FindGameObjectsWithTag("Card"))
            card.GetComponent<Card>().OnPlayedCard += OnCardPlayed;
    }

    void OnDisable()
    {
        UnSubScribeToEvents();
    }
    private void UnSubScribeToEvents()
    {
        DrawDeck_S.OnDrawCard -= OnDrawCard;
        foreach (GameObject card in GameObject.FindGameObjectsWithTag("Card"))
            card.GetComponent<Card>().OnPlayedCard -= OnCardPlayed;
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
    
    private void OnCardPlayed(Card playedCard)
    {
        ReArrangeDeck();
        ChangePlayerTurn(playedCard);
    }
    private void OnDrawCard()
    {
        ChangeTurns(1);
    }

    private void ReArrangeDeck()
    {
        GameObject.Find($"PlayerDeck{DeckCount}").GetComponent<PlayerDeck>().SortDeck();
    }
    private void ChangePlayerTurn(Card card)
    {
        switch (card.Value)
        {
            case "reverse": ReverseMode ^= true; ChangeTurns(1); break;
            case "cancel": ChangeTurns(2); break;
            case "plus2": DrawCards(2); break;
            case "plus4": DrawCards(4); break;
            case "plus6": DrawCards(6); break;
            case "wild": OnWildPlay(); break;
            case "wild4": OnWildPlay(); break;
            default: ChangeTurns(1); break;
        }
    }
    private void ChangeTurns(int amount)
    {
        while (amount != 0)
        {
            DeckCount += ReverseMode ? -1 : 1;

            if (DeckCount <= -1)
                DeckCount = NumberOfDecks-1;
            else if (DeckCount >= NumberOfDecks)
                DeckCount = 0;
            
            --amount;
        }
        SetCamera();
    }
    private void DrawCards(int amount)
    {
        ChangeTurns(1);
        PlayerDeck playerDeck = GameObject.Find($"PlayerDeck{DeckCount}").GetComponent<PlayerDeck>();

        StartCoroutine(DrawCardsWithDelay(amount, playerDeck));
    }
    private IEnumerator DrawCardsWithDelay(int amount, PlayerDeck playerDeck)
    {
        for (int i = 0; i < amount; ++i)
        {
            yield return new WaitForSeconds(1f); // Delay here
            DrawDeck_S.DrawCard(playerDeck.transform);
        }
        ChangeTurns(1);
    }

    private void OnWildPlay()
    {
        GameObject wildSelector = this.transform.GetChild(0).gameObject;

        wildSelector.SetActive(true);
    }
    public void OnWildSelect(Card card)
    {
        this.transform.GetChild(0).gameObject.SetActive(false);
        if (card.Value == "wild")
            ChangeTurns(1);

        else if (card.Value == "wild4")
            DrawCards(4);
        
    }

}
