using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    private static ChessBoard ChessBoard_S;
    
    public NetworkVariable<bool> IsP1Turn_Net = new NetworkVariable<bool>(true, NVRP.Everyone, NVWP.Server);
    public NetworkVariable<FixedString64Bytes> Board_Net = new NetworkVariable<FixedString64Bytes>("", NVRP.Everyone, NVWP.Server);

    void Start()
    {
        ChessBoard_S = GameObject.Find("ChessBoard").GetComponent<ChessBoard>();
    }

    public override void OnNetworkSpawn()
    {
        Board_Net.Value = "22222222222200000000111111111111";

        Board_Net.OnValueChanged += (FixedString64Bytes previousVal, FixedString64Bytes newVal) => {
            print("Old Value ::: "+ previousVal);
            print("New Value ::: "+ newVal);
        };
    }

    public bool ValidateBoard(FixedString64Bytes localNotation)
    {
        print("LocalNotation : " + localNotation);
        print("BoardNet : " + Board_Net.Value);
        print(localNotation == Board_Net.Value);

        return localNotation == Board_Net.Value;
    }

    public void SetBoardState(FixedString64Bytes newBoard)
    {
        Board_Net.Value = newBoard;
    }

    public void MakeMove(ulong callerID, Vector3 previousPos, Vector3 newPos)
    {
        print("MakeMove");
        ChessBoard_S.UpdatePos_ClientRpc(callerID, previousPos, newPos);
    }



    public static class NVRP
    {
        public static NetworkVariableReadPermission Owner => NetworkVariableReadPermission.Owner;
        public static NetworkVariableReadPermission Everyone => NetworkVariableReadPermission.Everyone;
    }
    public static class NVWP
    {
        public static NetworkVariableWritePermission Server => NetworkVariableWritePermission.Server;
        public static NetworkVariableWritePermission Owner => NetworkVariableWritePermission.Owner;
    }
}
