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


            Logger.Log($"Ended game after {turns} turns");
            PrintPGN(board);
        }

        [Test]
        public void InvalidMovesTest() {
            var badPGN = "1.a4 d5 2.h3 e6 3.h4 Ne7 4.Na3 b6 5.f3 Bd7 6.c4 d4 7.Kf2 h6 8.c5 bxc5 9.Qc2 Na6 10.Rb1 Nd5 11.Qe4 Bxa4 12.d3 h5 13.Nh3 Rc8 14.Ng1 Ne7 15.b3 Bd7 16.e3 f6 17.Bb2 c6 18.Rh3 Rg8 19.Bc1 Kf7 20.g3 Rh8 21.Bg2 Kg8 22.Bd2 Ra8 23.Rh1 Rb8 24.exd4 cxd4 25.Qxd4 Nc8 26.Nc4 Ne7 27.Na5 Nd5 28.Qxa7 Rb5 29.Rf1 c5 30.b4 cxb4 31.Nc4 b3 32.Qxa6 Qe8 33.Be1 Qb8 34.d4 f5 35.Bd2 g6 36.Nh3 Qe8 37.Qa1 Bg7 38.Ke1 Rc5 39.Na3";
            var parser = new PGNParser();
            parser.ParseSingleLine(badPGN);
            var board = parser.Board;
            var move = AIManager.GetMove(board, ChessPiece.EColour.Black, AIManager.MoveType.Standard);
            Logger.Log(move.ToString());
            Assert.IsTrue(board.Move(move));
        }

        void PrintPGN(ChessBoard board) {
            var moves = board.UndoStack.ToArray();
            var pgn = PGNExporter.ToPGN(board);
            Logger.Log(pgn);
        }
    }

}