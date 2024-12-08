using System.Collections.Generic;
using UnityEngine;

public class CombinationGame : MonoBehaviour
{
    public List<string> MoveList = new List<string>();

    public void AddToMoveList(string move)
    {
        MoveList.Add(move);
    }
}
