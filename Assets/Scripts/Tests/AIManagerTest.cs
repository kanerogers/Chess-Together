using UnityEngine;
using NUnit.Framework;

namespace Tests {
    [TestFixture]
    public class AIManagerTest {
        [Test]
        public void BasicAITest() {
            var board = new ChessBoard();
            var turn = 0;
            var canMove = ChessPiece.EColour.Black;
            var moveType = AIManager.MoveType.Standard;
            var turns = 999;

            while (turn < turns) {
                var currentPlayer = canMove;
                {
                    var state = board.State[currentPlayer];
                    if (state == ChessBoard.BoardStatus.Checkmate) {
                        Debug.Log($"Checkmate with board state: {board}");
                        Assert.Pass($"{currentPlayer} is in checkmate at turn {turn}!");
                    }

                    if (state == ChessBoard.BoardStatus.Stalemate) {
                        Debug.Log($"Stalemate with board state: {board}");
                        Assert.Pass($"{currentPlayer} is in stalemate at turn {turn}!");
                    }

                    var move = AIManager.GetMove(board, currentPlayer, moveType);

                    Assert.NotNull(move);
                    var piece = board.Pieces[move.ToRow, move.ToColumn];

                    if (piece != null) {
                        if (piece.Name == ChessPiece.EName.King) {
                            Debug.Log($"Ended with board state: {board}");
                            Assert.Fail($"Attempted to take a King with {move}");
                        }
                    }

                    if (!board.Move(move)) {
                        var badPiece = board.Pieces[move.FromRow, move.FromColumn];
                        Debug.Log($"Ended with board state: {board}");
                        Assert.Fail($"Tried to make invalid move {move} on turn {turn}");
                    }

                    state = board.State[currentPlayer];
                    if (state == ChessBoard.BoardStatus.Checkmate) {
                        Debug.Log($"Ended with board state: {board}");
                        Assert.Fail($"{currentPlayer} put itself into checkmate at {turn}!");
                    }
                    Logger.Log("AI_TEST", turn, "was", move);
                }

                turn++;
                canMove = canMove.Inverse();
            }

            Debug.Log($"Ended with board state: {board}");
        }
    }
}