using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] private CardSO Card_SO;
    private char Value;

    void OnAwake()
    {
        
    }

    void Start()
    {
        // this.transform.position = Camera.main.transform.position + new Vector3(0, 80f, 80f);
        // print(Camera.main.transform.position + new Vector3(0, 0f, 80f));
    }

    
    public void SetColor()
    {
        Material[] mats = new Material[4] {Card_SO.ColorRed, Card_SO.ColorYellow, Card_SO.ColorGreen, Card_SO.ColorCyan};
        
        this.GetComponent<MeshRenderer>().material = mats[Random.Range(0, mats.Length)];
    }
    public void SetValue(char value)
    {
        this.Value = value;
    }
}
