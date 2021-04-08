using NUnit.Framework;
using UnityEngine;
namespace IntegrationTests {
    public class PGNParserTest {
        [Test]
        public void ReadOneGame() {
            var parser = new PGNParser("Assets\\Scripts\\Tests\\test.pgn");
            var games = parser.Parse(1);
            Assert.AreEqual(1, games.Count);
            var moves = games[0];
            Assert.AreEqual(75, moves.Count);
        }

        [Test]
        public void ReadAll() {
            var parser = new PGNParser("Assets\\Scripts\\Tests\\test.pgn");
            var games = parser.Parse();
            Assert.AreEqual(3253, games.Count);
            ChessBoard board = null;
            var gameIndex = 0;
            var moveIndex = 0;
            try {
                foreach (var game in games) {
                    board = new ChessBoard();
                    foreach (var move in game) {
                        // Make move
                        board.Move(move);

                        // Undo it
                        board.Undo();

                        // Make move
                        board.Move(move);

                        var state = board.State[board.CanMove];
                        if (state == ChessBoard.BoardStatus.Checkmate || state == ChessBoard.BoardStatus.Stalemate) {
                            continue;
                        }

                        // Play an AI move
                        var aiMove = AIManager.GetMove(board, board.CanMove, AIManager.MoveType.Standard);
                        board.Move(aiMove);

                        // Undo it
                        board.Undo();

                        // Onto the next one..
                        moveIndex += 1;
                    }
                    gameIndex += 1;
                }
            } catch (System.Exception e) {
                Debug.Log($"Encountered error at game {gameIndex}");
                Debug.LogError(e.ToString());
                Debug.LogError(PGNExporter.ToPGN(board));
                throw e;
            }
        }
    }
}