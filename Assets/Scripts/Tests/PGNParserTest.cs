using NUnit.Framework;
using UnityEngine;
namespace Tests {
    public class PGNParserTest {
        [Test]
        public void ReadOneGame() {
            var parser = new PGNParser("Assets\\Scripts\\Tests\\test.pgn");
            var games = parser.Parse(1);
            Assert.AreEqual(1, games.Count);
            var moves = games[0];
            Assert.AreEqual(71, moves.Count);
        }

        [Test]
        public void ReadAll() {
            var parser = new PGNParser("Assets\\Scripts\\Tests\\test.pgn");
            var games = parser.Parse(1);
            // Assert.AreEqual(3252, games.Count);
            ChessBoard board = null;
            var gameIndex = 0;
            var moveIndex = 0;
            try {
                foreach (var game in games) {
                    board = new ChessBoard();
                    var canMove = ChessPiece.EColour.White;
                    foreach (var move in game) {
                        if (moveIndex == 38) {
                            Logger.AT_CORRECT_MOVE = true;
                        }
                        // Make move
                        board.Move(move);

                        // Undo it
                        board.Undo();

                        // Make move
                        board.Move(move);

                        var blackState = board.State[ChessPiece.EColour.Black];
                        var whiteState = board.State[ChessPiece.EColour.White];
                        if (blackState == ChessBoard.BoardStatus.Checkmate || blackState == ChessBoard.BoardStatus.Stalemate || whiteState == ChessBoard.BoardStatus.Checkmate || whiteState == ChessBoard.BoardStatus.Stalemate) {
                            continue;
                        }

                        if (board.ValidMoves[canMove].Count > 0) {
                            // Make the AI think
                            AIManager.GetMove(board, canMove, AIManager.MoveType.Standard);
                        }

                        if (blackState == ChessBoard.BoardStatus.Checkmate || blackState == ChessBoard.BoardStatus.Stalemate || whiteState == ChessBoard.BoardStatus.Checkmate || whiteState == ChessBoard.BoardStatus.Stalemate) {
                            Logger.Log("heya");
                        }


                        // Play an AI move
                        var aiMove = AIManager.GetMove(board, canMove.Inverse(), AIManager.MoveType.Standard);
                        board.Move(aiMove);

                        // Undo it
                        board.Undo();

                        canMove = canMove.Inverse();

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