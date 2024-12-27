// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.IO;
// using UnityEngine;

// public class stockfishAI : MonoBehaviour
// {
//     private Process stockfishProcess;
//     private StreamWriter stockfishInput;
//     private StreamReader stockfishOutput;

//     // Path to the Stockfish binary
//     private string stockfishPath = Application.dataPath+"/stockfish/stockfish-windows-x86-64-avx2.exe";
//     private int Difficulty = 6;
//     private float MoveTimer = 2f; //in seconds

//     public bool[] CanCastle1 = new bool[3] {true, true, true}; // King, king rook, queen rook
//     public bool[] CanCastle2 = new bool[3] {true, true, true};
//     public Pawn EnPassantPiece;
//     public string MoveSequence = "";
//     private ChessBoard ChessBoard_S;

//     // 1 800
//     // 2 1100
//     // 3 1400 
//     // 4 1700
//     // 5 2000
//     // 6 2300
//     // 7 2700
//     // 8 3000


//     void Start()
//     {
//         ChessBoard_S = this.GetComponent<ChessBoard>();
        
//         MoveTimer *= 1000;
//         StartStockfish();
//     }

//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Space))
//             ChessBoard.BoardToFenNotation();
//     }

//     // Method to start Stockfish process
//     private void StartStockfish()
//     {
//         try{
//             stockfishProcess = new Process
//             {
//                 StartInfo = new ProcessStartInfo
//                 {
//                     FileName = stockfishPath,
//                     RedirectStandardInput = true,
//                     RedirectStandardOutput = true,
//                     UseShellExecute = false,
//                     CreateNoWindow = true
//                 }
//             };

//             stockfishProcess.Start();

//             stockfishInput = stockfishProcess.StandardInput;
//             stockfishOutput = stockfishProcess.StandardOutput;

//             // Initialize UCI
//             SendCommand("uci");
//             print(ReadStockfishOutput()); // Wait for 'uci ok'
//             SendCommand($"setoption name Skill Level value {Difficulty}");
//         }
//         catch (Exception ex){
//             print("Error starting Stockfish: " + ex.Message);
//         }
//     }

//     // Method to send commands to Stockfish
//     private void SendCommand(string command)
//     {
//         if (stockfishProcess != null && !stockfishProcess.HasExited)
//         {
//             stockfishInput.WriteLine(command);
//             stockfishInput.Flush();
//         }
//     }

//     // Method to read output from Stockfish
//     private string ReadStockfishOutput()
//     {
//         return stockfishOutput.ReadLine();
//     }

//     // Method to get the best move for the current position
//     public string GetBestMove(string position)
//     {
//         string theCommand = $"position fen {position} moves {MoveSequence}";
//         print(theCommand);
//         SendCommand(theCommand);
//         SendCommand($"go movetime {MoveTimer}");
//         string output;

//         while ((output = ReadStockfishOutput()) != null)
//             if (output.StartsWith("bestmove"))
//                 break;
        
//         print(output);
//         return output;
//     }

//     // Ensure the Stockfish process is terminated when the game stops
//     private void OnApplicationQuit()
//     {
//         if (stockfishProcess != null && !stockfishProcess.HasExited)
//         {
//             stockfishProcess.Kill();
//         }
//     }



//     private string GetTablePosition()
//     {
//         string fenNotation = TablePosition() + PlayerTurn() + CastleRights() + PassantPieceLocation() + TurnCounts();
//         return fenNotation;

//         string TablePosition()
//         {return ChessBoard.BoardToFenNotation();}
//         string PlayerTurn()
//         {return BoardMaterials.IsP1Turn ? " w " : " b ";}
//         string CastleRights()
//         {
//             string castleString = "";

//             if (!CanCastle1[0] && !CanCastle2[0])
//                 return "- ";

//             if (CanCastle1[0])
//             {
//                 if (CanCastle1[1])
//                     castleString += "K";
//                 if (CanCastle1[2])
//                     castleString += "Q";
//             }
//             if (CanCastle2[0])
//             {
//                 if (CanCastle2[1])
//                     castleString += "k";
//                 if (CanCastle2[2])
//                     castleString += "q";
//             }
//             return castleString + " ";
//         }
//         string PassantPieceLocation()
//         {
//             if (!EnPassantPiece)
//                 return "- ";
            
//             string pos = GenericPiece.BoardPosToAlgebraNotation(ChessBoard.PosToBoardPos(EnPassantPiece.transform.position).ToString());

//             EnPassantPiece = null;
//             return pos + " ";
//         }
//         string TurnCounts()
//         {return ChessBoard_S.HalfMove + " " + ChessBoard_S.TurnCount;}
//     }

//     private void movePiece(string move)
//     {
//         print(move);
//         string oldPos = move.Substring(0,2);
//         string newPos = move.Substring(2,2);

//         Vector3 oldPosition = ChessBoard.BoardPosToPos(GenericPiece.AlgebraNotationToBoardPos(oldPos));
//         Vector3 newPosition = ChessBoard.BoardPosToPos(GenericPiece.AlgebraNotationToBoardPos(newPos));

//         print($"move: {oldPos} | {newPos}");
//         print(GenericPiece.AlgebraNotationToBoardPos(oldPos));

//         GenericPiece piece = ChessBoard.Board[GenericPiece.AlgebraNotationToBoardPos(oldPos)];

//         piece.ProcessTurnLocally(new Vector3[2] {oldPosition, newPosition});

//         // GameObject[] allPieces = GameObject.FindGameObjectsWithTag("Piece");
//         // foreach (GameObject piece in allPieces)
//         // {
//         //     if (piece.transform.position == positions[0])
//         //     {
//         //         piece.GetComponent<GenericPiece>().ProcessTurnLocally(new Vector3[2] {positions[0], positions[1]});
//         //         return;
//         //     }
//         // }

//         // GenericChessPiece piece = GenericPiece.FindPieceObject(oldX, oldY).GetComponent<GenericChessPiece>();
//         // piece.transform.position = new Vector2(newX, newY);
//         // piece.MULT_OnMouseUP();
        
//         // if (piece is Pawn && move[4]!=' ')
//         //     piece.GetComponent<Pawn>().CheckPromotion(move[4]);
//     }

//     public void MakeMove()
//     {
//         List<string> moves = ChessBoard_S.MoveList;
//         MoveSequence += moves[moves.Count-1];

//         string tablePos = GetTablePosition();
//         string bestMove = GetBestMove(tablePos).Substring(9, 4);
//         movePiece(bestMove);

//         MoveSequence += " " + bestMove + " ";
//     }

// }
