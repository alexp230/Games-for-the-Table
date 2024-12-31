using UnityEngine;
using TMPro;

public class Card : MonoBehaviour
{
    [SerializeField] private CardSO Card_SO;

    [SerializeField] private TextMeshProUGUI MiddleText;
    [SerializeField] private TextMeshProUGUI TopLeftText;
    [SerializeField] private TextMeshProUGUI BottomRightText;
    [SerializeField] private BoxCollider BoxCollider;

    private char Value;
    private int CardLift = 20;

    void Start()
    {
        this.SetColor();
        this.SetValue();
    }

    void OnMouseEnter()
    {
        Vector3 pos = this.transform.position;
        this.transform.position = new Vector3(pos.x, pos.y+CardLift, pos.z);

        Vector3 boxPos = this.BoxCollider.size;
        print(this.BoxCollider.size);
        this.BoxCollider.size = new Vector3(boxPos.x, boxPos.y*2, boxPos.z);
    }

    void OnMouseExit()
    {
        Vector3 pos = this.transform.position;
        this.transform.position = new Vector3(pos.x, pos.y-CardLift, pos.z);

        Vector3 boxPos = this.BoxCollider.size;
        print(this.BoxCollider.size);
        this.BoxCollider.size = new Vector3(boxPos.x, boxPos.y/2, boxPos.z);
    }

    
    private void SetColor()
    {
        Material[] mats = new Material[4] {Card_SO.ColorRed, Card_SO.ColorYellow, Card_SO.ColorGreen, Card_SO.ColorCyan};
        
        this.GetComponent<MeshRenderer>().material = mats[Random.Range(0, mats.Length)];
    }
    private void SetValue()
    {
        string value = Random.Range(1, 10).ToString();
        
        this.Value = value[0];
        MiddleText.text = value;
        TopLeftText.text = value;
        BottomRightText.text = value;
    }
}
