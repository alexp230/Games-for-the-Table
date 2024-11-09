using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class Checker : NetworkBehaviour
{
    [SerializeField] protected BoardMaterials CheckerMaterials_SO;
    [SerializeField] protected MeshRenderer _MeshRenderer;
    private static ChessBoard ChessBoard_S;
    private static NetworkGameManager NetworkGameManager_S;
    public const float Y = 0.15f;

    private static Camera MainCamera;
    private static float CameraDistanceZ;

    [SerializeField] public Vector3 PreviousPosition { get; protected set; }
    [SerializeField] public int TeamID { get; protected set; }
    [SerializeField] public List<int> ValidMoves { get; private set; } = new List<int>();

    private void Awake()
    {
        Vector3 pos = this.transform.position;
        bool isP1 = ChessBoard.IsP1Turn;

        this._MeshRenderer.material = isP1 ? CheckerMaterials_SO.Piece_p1Color : CheckerMaterials_SO.Piece_p2Color;
        this.PreviousPosition = pos;
        this.TeamID = (this is Duke) ? (isP1 ? ChessBoard.P1_DUKE : ChessBoard.P2_DUKE) : (isP1 ? ChessBoard.P1_PIECE : ChessBoard.P2_PIECE);

        int index = ChessBoard.PosToBoardPos(pos);
        ChessBoard.Board[index] = this;
    }

    public override void OnNetworkSpawn()
    {
        this._MeshRenderer.material = ChessBoard.IsP1Turn ? CheckerMaterials_SO.Piece_p1Color : CheckerMaterials_SO.Piece_p2Color;

        base.OnNetworkSpawn();
    }

    private void Start()
    {
        MainCamera = Camera.main;
        CameraDistanceZ = MainCamera.WorldToScreenPoint(this.transform.position).z;
        ChessBoard_S = GameObject.Find("ChessBoard").GetComponent<ChessBoard>();
        NetworkGameManager_S = GameObject.Find("NetworkGameManager").GetComponent<NetworkGameManager>();
    }


    private bool CanMove()
    {
        bool p1Turn = ChessBoard_S.IsP1Turn_Net.Value;
        ulong playerID = NetworkManager.Singleton.LocalClientId;

        if ((!p1Turn && playerID == 0) || (p1Turn && playerID == 1))
            return false;
        if (this.ValidMoves.Count == 0)
            return false;

        return true;
    }

    private void OnMouseDown()
    {
        if (!CanMove())
            return;
        
        this.PreviousPosition = RoundVector(this.transform.position);
        Tile.HighlightTiles(this.ValidMoves);
    }

    private void OnMouseDrag()
    {
        if (!CanMove())
            return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 screenPos = new Vector3(mousePos.x, mousePos.y, CameraDistanceZ);
        Vector3 newWorldPos = MainCamera.ScreenToWorldPoint(screenPos);
        
        this.transform.position = newWorldPos;
    }

    private void OnMouseUp()
    {
        if (!CanMove())
            return;
            
        Vector3 currentPos = RoundVector(this.transform.position);
        this.transform.position = this.PreviousPosition;

        foreach (int validMove in ValidMoves)
        {
            Vector3 validPos = ChessBoard.BoardPosToPos(validMove);
            if (currentPos == validPos)
            {
                // ChessBoard_S.ValidateGame();

                ChessBoard_S.MakeThatMove(this.PreviousPosition, validPos);

                ChessBoard_S.UpdateBoard(this.PreviousPosition, validPos);
                UpdatePosition(this, validPos);
                DoSecondJumpOrChangeSides(this, validPos);                

                this.PreviousPosition = validPos;
                // UpdatePos_ClientRpc(this.PreviousPosition, validPos);
                break;
            }
        }
        
        Tile.DeHighLightTiles();
    }


    private void UpdatePosition(Checker currentPiece, Vector3 validPos)
    {
        currentPiece.transform.position = validPos;
    }
    private void UpdateGameState()
    {
        ChessBoard_S.SetGameState();
    }
    private void DoSecondJumpOrChangeSides(Checker currentPiece, Vector3 validPos)
    {
        List<int> newValidMoves = GetValidMoves(this, getOnlyJumps: true); // Check for second jump
        if (newValidMoves.Count > 0 && Math.Abs(currentPiece.PreviousPosition.x - validPos.x) == 2) // if piece has move and made a jump
        {
            ChessBoard_S.ClearAllPiecesValidMoves();
            currentPiece.ValidMoves = newValidMoves;
        }
        else
            ChessBoard_S.ChangeSides();
    }


    [ServerRpc(RequireOwnership = false)]
    public void MakeMove_ServerRPC(Vector3 currentPos, Vector3 newPos)
    {
        print("ServerRPC");
        NetworkGameManager_S.MakeMove(OwnerClientId, currentPos, newPos);
    }







    protected List<int> GetValidMoves(Checker currentPiece, bool getOnlyJumps)
    {
        Checker[] board = ChessBoard.Board;
        int[] offsets = (this is Duke) ? new int[]{-9,-7,7,9} : (this.TeamID%2 == ChessBoard.P1_PIECE) ? new int[]{-9, -7} : new int[]{7, 9};
        int boardPos = ChessBoard.PosToBoardPos(RoundVector(currentPiece.transform.position));

        List<int> allMoves = new List<int>();
        foreach (int offset in offsets)
        {
            int newPos = boardPos+offset;
            if (OutOfBounds(newPos) || Overflown(newPos, offset))
                continue;

            Checker newPiece = board[newPos];
            if (!newPiece)
            {
                if (!getOnlyJumps)
                    allMoves.Add(newPos);
                continue;
            }

            if (newPiece.TeamID%2 == currentPiece.TeamID%2)
                continue;
            
            newPos += offset;
            if (OutOfBounds(newPos) || Overflown(newPos, offset))
                continue;

            if (board[newPos] == null)
                allMoves.Add(newPos);
        }
        return allMoves;
    
        bool OutOfBounds(int currentPos){return currentPos < 0 || currentPos > 63;}
        bool Overflown(int currentPos, int offset)
        {
            // 0  1  2  3  4  5  6  7
            // 8  9  10 11 12 13 14 15
            // 16 17 18 19 20 21 22 23
            // 24 25 26 27 28 29 30 31
            // 32 33 34 35 36 37 38 39
            // 40 41 42 43 44 45 46 47
            // 48 49 50 51 52 53 54 55
            // 56 57 58 59 60 61 62 63

            // i.e Given the tile 7, when trying to access its right tile, it will get 16 which is not a valid tile for
            // the piece on 7 to go to

            int[] rightOverflow = new int[8]{ 0,8,16,24,32,40,48,56 };
            int[] leftOverflow = new int[8]{ 7,15,23,31,39,47,55,63 };

            foreach (int val in rightOverflow)
                if (val == currentPos && (offset == -7 || offset == 9))
                    return true;
            foreach (int val in leftOverflow)
                if (val == currentPos && (offset == -9 || offset == 7))
                    return true;
            return false;
        }
      
    }



    public void UpdatePreviousPos(Vector3 position)
    {
        this.PreviousPosition = position;
    }
    public void SetValidMoves()
    {
        this.ValidMoves = GetValidMoves(this, getOnlyJumps: false);
    }
    public void SetValidMoves(List<int> moves)
    {
        this.ValidMoves = moves;
    }
    public void ClearValidMoves()
    {
        this.ValidMoves.Clear();
    }





    private static Vector3 RoundVector(Vector3 vector)
    {
        return new Vector3(Mathf.RoundToInt(vector.x), Y, Mathf.RoundToInt(vector.z));
    }

}
