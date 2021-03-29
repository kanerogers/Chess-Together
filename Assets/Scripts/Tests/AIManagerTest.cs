using UnityEngine;
using NUnit.Framework;

namespace Tests {
    [TestFixture]
    public class AIManagerTest {
        [Test]
        public void BasicAITest() {
            var board = new ChessBoard();
            var turn = 0;
            var canMove = ChessPiece.EColour.White;
            var moveType = AIManager.MoveType.Standard;
            var turns = 400;

            while (turn < turns) {
                var currentPlayer = canMove;
                {
                    var state = board.State[currentPlayer];
                    if (state == ChessBoard.BoardStatus.Checkmate) {
                        PrintPGN(board);
                        Assert.Pass($"{currentPlayer} is in checkmate at turn {turn}!");
                    }

                    if (state == ChessBoard.BoardStatus.Stalemate) {
                        PrintPGN(board);
                        Assert.Pass($"{currentPlayer} is in stalemate at turn {turn}!");
                    }

                    Move move = null;
                    try {
                        move = AIManager.GetMove(board, currentPlayer, moveType);
                    } catch (System.Exception e) {
                        PrintPGN(board);
                        throw e;
                    }

                    Assert.NotNull(move);
                    var piece = board.Pieces[move.ToRow, move.ToColumn];

                    if (piece != null) {
                        if (piece.Name == ChessPiece.EName.King) {
                            PrintPGN(board);
                            Assert.Fail($"Attempted to take a King with {move}");
                        }
                    }

                    if (!board.Move(move)) {
                        var badPiece = board.Pieces[move.FromRow, move.FromColumn];
                        PrintPGN(board);
                        Assert.Fail($"Tried to make invalid move {move} on turn {turn}");
                    }

                    state = board.State[currentPlayer];
                    if (state == ChessBoard.BoardStatus.Checkmate) {
                        PrintPGN(board);
                        Assert.Fail($"{currentPlayer} put itself into checkmate at {turn}!");
                    }
                    Logger.Log("AI_TEST", turn, "was", move);
                }

                turn++;
                canMove = canMove.Inverse();
            }


            Debug.Log($"Ended game after {turns} turns");
            PrintPGN(board);
        }
        void PrintPGN(ChessBoard board) {
            var pgn = PGNExporter.ToPGN(board);
            Debug.Log(pgn);
        }
    }

}