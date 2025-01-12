using UnityEngine;

public class ColorSelector : MonoBehaviour
{
    private PlayPile PlayPile_S;

    void Start()
    {
        PlayPile_S = GameObject.Find("PlayPile").GetComponent<PlayPile>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            OnSelectColor("Red");
        else if (Input.GetKeyDown(KeyCode.G))
            OnSelectColor("Green");
        else if (Input.GetKeyDown(KeyCode.B))
            OnSelectColor("Cyan");
        else if (Input.GetKeyDown(KeyCode.Y))
            OnSelectColor("Yellow");
    }

    private void OnSelectColor(string color)
    {
        Card card = PlayPile_S.transform.GetChild(PlayPile_S.transform.childCount-1).GetComponent<Card>();
        card.SetColor(color);
        this.transform.parent.transform.parent.GetComponent<Ichi>().OnWildSelect(card);
    }
}
