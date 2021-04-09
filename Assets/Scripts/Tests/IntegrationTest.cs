using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

namespace IntegrationTests {
    public class IntegrationTest {
        IEnumerator LoadScene() {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Main");

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone) {
                yield return null;
            }
        }
        public GameManager GetGameManager() {
            var gm = GameObject.Find("Game Manager");
            return gm.GetComponent<GameManager>();
        }

        IEnumerator StartAIGame(GameManager gameManager) {
            // We gotta wait for the splash screen to finish
            var boardInterfaceManager = gameManager.boardInterfaceManager;
            yield return new WaitForSeconds(5);
            boardInterfaceManager.LeftButtonPressed();
            yield return null;
        }

        IEnumerator MovePiece(SceneChessBoard sceneBoard, Move move) {
            var (fromRow, fromColumn, toRow, toColumn) = move.ToCoordinates();
            // Step one, pick up a piece
            var piece = sceneBoard.Pieces[fromRow, fromColumn];

            // Step two, mark it as grabbed
            var scenePiece = piece.GetComponent<SceneChessPiece>();
            scenePiece.Grabbed();

            // Step three, move it in space to where it should go
            var newPosition = sceneBoard.CoordinatesForPosition(toRow, toColumn);
            newPosition.y = 0.1f;
            piece.transform.localPosition = newPosition;

            // Wait for it to move
            yield return new WaitForSeconds(1f);

            // Step four, release it
            scenePiece.Released();

            yield return new WaitForSeconds(sceneBoard.MovementDuration + 1f);
        }

        [UnityTest]
        public IEnumerator FullMovementTest() {
            yield return LoadScene();
            var gameManager = GetGameManager();
            yield return StartAIGame(gameManager);
            var sceneBoard = gameManager.SceneBoard;

            var move = new Move(6, 0, 4, 0);
            yield return MovePiece(sceneBoard, move);

            // Step five, assert it's in the right place.
            var pawn = sceneBoard.Pieces[4, 0].GetComponent<SceneChessPiece>();
            Assert.AreEqual(ChessPiece.EName.Pawn, pawn.Piece.Name);
            Assert.AreEqual(ChessPiece.EColour.White, pawn.Piece.Colour);
            Assert.AreEqual(4, pawn.Piece.Row);
            Assert.AreEqual(0, pawn.Piece.Column);
        }

        [UnityTest]
        [Timeout(100000000)]
        public IEnumerator AIGameTest() {
            yield return LoadScene();
            var gameManager = GetGameManager();
            yield return StartAIGame(gameManager);
            var sceneBoard = gameManager.SceneBoard;

            var openingMove = new Move(6, 0, 4, 0);
            yield return MovePiece(sceneBoard, openingMove);
            var logicBoard = gameManager.LogicBoard;
            bool canMove = false;
            bool finished = false;


            EventManager.MoveComplete += (ChessPiece.EColour justMoved) => {
                Debug.Log($"Just moved: {justMoved}");
                if (justMoved.IsBlack()) {
                    var whiteState = logicBoard.State[ChessPiece.EColour.White];
                    if (whiteState == ChessBoard.BoardStatus.Checkmate || whiteState == ChessBoard.BoardStatus.Stalemate) {
                        finished = true;
                    }

                    canMove = true;
                }
            };

            while (!finished) {
                while (!canMove) yield return null;
                if (finished) yield break;
                canMove = false;
                Move nextMove;
                try {
                    nextMove = AIManager.GetMove(logicBoard, ChessPiece.EColour.White, AIManager.MoveType.Standard);
                } catch (System.Exception e) {
                    var pgn = PGNExporter.ToPGN(logicBoard);
                    Debug.Log(pgn);
                    throw e;
                }

                yield return NextTurn(gameManager, nextMove);

                var blackState = logicBoard.State[ChessPiece.EColour.Black];
                if (blackState == ChessBoard.BoardStatus.Checkmate || blackState == ChessBoard.BoardStatus.Stalemate) {
                    finished = true;
                }
            }
        }

        [UnityTest]
        [Timeout(100000000)]
        public IEnumerator PGNGameTest() {
            yield return LoadScene();
            var gameManager = GetGameManager();
            yield return StartAIGame(gameManager);
            var sceneBoard = gameManager.SceneBoard;
            gameManager.opponentType = GameManager.OpponentType.None;

            var logicBoard = gameManager.LogicBoard;
            bool canMove = true;
            var parser = new PGNParser("Assets\\Scripts\\Tests\\test.pgn");
            var games = parser.Parse(1);
            var moves = games[0];
            var turn = 0;

            EventManager.MoveComplete += (ChessPiece.EColour justMoved) => {
                turn += 1;
                Debug.Log($"Just moved: {justMoved}");
                canMove = true;
            };

            while (turn != moves.Count) {
                while (!canMove) yield return null;
                canMove = false;
                var move = moves[turn];
                yield return NextTurn(gameManager, move);
            }
        }


        private IEnumerator NextTurn(GameManager gameManager, Move nextMove) {
            var logicBoard = gameManager.LogicBoard;
            var sceneBoard = gameManager.SceneBoard;
            var canMove = logicBoard.CanMove;


            Debug.Log($"[{logicBoard.Turn}] Making {nextMove}..");
            var (fromRow, fromColumn, toRow, toColumn) = nextMove.ToCoordinates();
            var expectedPieceName = sceneBoard.Pieces[fromRow, fromColumn].GetComponent<SceneChessPiece>().Piece.Name;
            yield return MovePiece(sceneBoard, nextMove);
            var piece = sceneBoard.Pieces[toRow, toColumn].GetComponent<SceneChessPiece>().Piece;

            Assert.AreEqual(expectedPieceName, piece.Name);
            Assert.AreEqual(canMove, piece.Colour);
            Assert.AreEqual(toRow, piece.Row);
            Assert.AreEqual(toColumn, piece.Column);

        }
    }
}