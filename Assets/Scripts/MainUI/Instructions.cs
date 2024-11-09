// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UIElements;

// public class Instructions : MonoBehaviour
// {
//     [SerializeField] private TextMeshProUGUI GameTitle;
//     [SerializeField] private TextMeshProUGUI DirectionsText;
//     [SerializeField] private Button PreviousButton;
//     [SerializeField] private TextMeshProUGUI PreviousButtonText;
//     [SerializeField] private Button NextButton;
//     [SerializeField] private TextMeshProUGUI NextButtonText;

//     private List<Direction> AllDirections = new List<Direction>();
//     private int CurrentIndex = 0;

//     void Start()
//     {
//         // string checkers = @"Each player take turns moving their piece up the board. Click and hold (left-click) to select your 
//         // piece to move and drag it to where you want to place it. Each piece can only move one space forward and diagonally, unless 
//         // an opponent's piece is on the square and the square beyond it is empty, in which you are able to 'jump' the piece by moving
//         // your piece behind the opponent's piece on the diagonal. Once a piece is 'jumped' it will be removed from the board. Once you
//         // have moved one of your pieces to the opponent's back row, the checker is elevated into a king and now gains the ability to 
//         // move backwards. The player who captures/jumps all of the other player pieces wins. If a player cannot make a legal move, 
//         // the player loses.";

//         string game = "Combination";
//         string instruction = $"<align=center>This game takes rules from both Checkers and Chess.</align>\n\n" +
//         "• You start the game with checkers on the board and every move you make (with a checker) generates one coin.\n" +
//         "• You are able to elevate a piece to a chess piece on your turn using your coins.\n" +
//         "• Each movement of a chess piece (except for the 'King') cost one coin.\n";
//         AllDirections.Add(new Direction(game, instruction));

//         instruction = $"• After {ChessBoard.KING_TIME} turns, each player will place their king on their back row.\n" +
//         "• Once placed, the first player to checkmate the opponent's king wins.\n" +
//         "• Checker pieces that reaches the opponent's back row are referred to as a 'Duke' but keep their checker functionality.";
//         AllDirections.Add(new Direction(game, instruction));

//         instruction = "Hover mouse over checker piece and press specific hotkey to elevate it.\n" + 
//         $"<align=center>Piece  | hotkey | price</align>\n" +
//         $"<align=center>Pawn | (P) | {ChessBoard.PAWN_PRICE}</align>\n" +
//         $"<align=center>Knight | (K) | {ChessBoard.KNIGHT_PRICE}</align>\n" +
//         $"<align=center>Bishop | (B) | {ChessBoard.BISHOP_PRICE}</align>\n" +
//         $"<align=center>Rook | (R) | {ChessBoard.ROOK_PRICE}</align>\n" +
//         $"<align=center>Queen | (Q) | {ChessBoard.QUEEN_PRICE}</align>\n";
//         AllDirections.Add(new Direction(game, instruction));


//         game = "Investore";
//         instruction = $"The goal is to earn as much profit as you can in the given amount of time.\n\n" +
//         "• Use the money earned from your side hustle to purchase and upgrade businesses to earn more money.\n" +
//         $"• Every {Investore.QnA_TIMER_INACTIVE} seconds, a math question will appear for {Investore.QnA_TIMER_ACTIVE} seconds " +
//         "for you to answer";
//         AllDirections.Add(new Direction(game, instruction));

//         instruction = "<align=center>These are the potential results after answering the question:\n\n</align>" +
//         $"• No answer = $0\n• Incorrect answer = ${Investore.QnA_WRONG}\n• Correct Answer = ${Investore.QnA_CORRECT}";
//         AllDirections.Add(new Direction(game, instruction));



//         Direction firstDirection = AllDirections[0];
//         GameTitle.text = firstDirection.Game;
//         DirectionsText.text = firstDirection.Instruction;  
        
//         setButtonsText();
//     }

//     public void OnDirectionChange(Button pressedButton)
//     {
//         List<Direction> allDirections = AllDirections;

//         CurrentIndex += (pressedButton == PreviousButton) ? -1 : 1;
//         CurrentIndex = Math.Clamp(CurrentIndex, 0, allDirections.Count-1);

//         GameTitle.text = allDirections[CurrentIndex].Game;
//         DirectionsText.text = allDirections[CurrentIndex].Instruction;

//         setButtonsText();
//     }

//     private void setButtonsText()
//     {
//         List<Direction> allDirections = AllDirections;

//         if (CurrentIndex != 0)
//         {
//             string previousGameTitle = allDirections[CurrentIndex-1].Game;
//             PreviousButtonText.text = (GameTitle.text == previousGameTitle) ? "Previous" : previousGameTitle; 
//         }
//         else
//             PreviousButtonText.text = "";

//         if (CurrentIndex != allDirections.Count-1)
//         {
//             string nextGameTitle = allDirections[CurrentIndex+1].Game;
//             NextButtonText.text = (GameTitle.text == nextGameTitle) ? "Next" : nextGameTitle;  
//         }
//         else
//             NextButtonText.text = "";
//     }


//     public class Direction
//     {
//         public string Game { get; set; }
//         public string Instruction { get; set; }

//         public Direction(string game, string instruction)
//         {
//             Game = game;
//             Instruction = instruction;
//         }
//     }
    
// }
