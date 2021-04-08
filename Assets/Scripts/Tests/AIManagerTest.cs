using UnityEngine;
using NUnit.Framework;

namespace UnitTests {
    [TestFixture]
    public class AIManagerTest {
        [Test]
        public void BasicAITest() {
            var board = new ChessBoard();
            var turn = 0;
            var moveType = AIManager.MoveType.Standard;
            var turns = 400;

            while (turns != 400) {
                var currentPlayer = board.CanMove;
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
            }


            Debug.Log($"Ended game after {turns} turns");
            PrintPGN(board);
        }
        void PrintPGN(ChessBoard board) {
            var moves = board.UndoStack.ToArray();
            var pgn = PGNExporter.ToPGN(board);
            Debug.Log(pgn);
        }
    }

}