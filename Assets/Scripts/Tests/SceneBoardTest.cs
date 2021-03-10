using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace Tests {
    public class SceneBoardTests {
        bool poolCreated = false;
        GameObject poolManager;
        public SceneChessBoard GetBoard() {
            // Create pool manager
            if (!poolCreated) {
                poolManager = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/Pool Manager"));
                poolCreated = true;
            }

            // Create Game Manager
            GameManager gameManager = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/Game Manager")).GetComponent<GameManager>();
            gameManager.opponentType = GameManager.OpponentType.AI;

            // Wire up all the hideous dependencies
            GameObject chessBoardGameObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/Chess Board"));
            BoardInterfaceManager bi = chessBoardGameObject.GetComponent<BoardInterfaceManager>();
            bi.GameManager = gameManager;
            gameManager.boardInterfaceManager = bi;
            var board = chessBoardGameObject.GetComponent<SceneChessBoard>();
            gameManager.SceneBoard = board;
            var logicBoard = new ChessBoard();
            board.InitializeBoard(logicBoard);
            gameManager.LogicBoard = logicBoard;
            return board;
        }

        [Test]
        public void BuildChessBoard() {
            var board = GetBoard();

            for (int r = 0; r < 8; r++) {
                for (int c = 0; c < 8; c++) {
                    GameObject obj = board.Pieces[r, c];

                    if (r == 0 && c == 0) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Rook);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        Assert.AreEqual(piece.Row, r);
                        Assert.AreEqual(piece.Column, c);
                        continue;
                    }

                    if (r == 0 && c == 1) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Knight);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        Assert.AreEqual(piece.Row, r);
                        Assert.AreEqual(piece.Column, c);
                        continue;
                    }

                    if (r == 0 && c == 2) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Bishop);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 0 && c == 3) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Queen);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 0 && c == 4) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.King);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 0 && c == 5) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Bishop);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 0 && c == 6) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Knight);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 0 && c == 7) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Rook);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 1) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Pawn);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        Assert.AreEqual(piece.Row, r);
                        Assert.AreEqual(piece.Column, c);
                        continue;
                    }

                    if (r == 6) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Pawn);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 0) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Rook);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 1) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Knight);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 2) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Bishop);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 3) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Queen);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 4) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.King);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 5) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Bishop);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 6) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Knight);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 7) {
                        var piece = obj.GetComponent<SceneChessPiece>().Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Rook);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    Assert.IsNull(obj);
                }
            }
        }
        [UnityTest]
        public IEnumerator TestMovement() {
            var board = GetBoard();
            var piece = board.Pieces[6, 0].GetComponent<SceneChessPiece>();
            Assert.IsTrue(board.Move(piece, 5, 0));

            piece = board.Pieces[5, 0].GetComponent<SceneChessPiece>();
            yield return new WaitForSeconds(board.MovementDuration + 0.1f);
            var expectedPosition = board.CoordinatesForPosition(5, 0);
            Assert.AreEqual(expectedPosition, piece.transform.localPosition);
        }

        [UnityTest]
        public IEnumerator TestPromotion() {
            var sceneBoard = GetBoard();
            var bi = sceneBoard.GetComponent<BoardInterfaceManager>();
            sceneBoard.boardInterfaceManager = bi;
            var board = new ChessBoard(true);
            board.CreatePiece(ChessPiece.EName.Pawn, 6, 0, ChessPiece.EColour.Black);
            board.CreatePiece(ChessPiece.EName.Pawn, 1, 0, ChessPiece.EColour.White);
            board.CreatePiece(ChessPiece.EName.King, 7, 4, ChessPiece.EColour.White);
            board.CreatePiece(ChessPiece.EName.King, 0, 4, ChessPiece.EColour.Black);

            board.BlackKing = (King)board.Pieces[0, 4];
            board.WhiteKing = (King)board.Pieces[7, 4];
            sceneBoard.LogicBoard = board;
            sceneBoard.SetUpBoard();

            var pawn = sceneBoard.Pieces[1, 0].GetComponent<SceneChessPiece>();
            sceneBoard.Move(pawn, 0, 0);

            Assert.AreEqual(bi.currentState, BoardInterfaceManager.State.ChoosingPieceToPromoteTo);
            bi.LeftButtonPressed();
            bi.RightButtonPressed();

            yield return new WaitForSeconds(sceneBoard.MovementDuration + 0.1f);
            var bishop = sceneBoard.Pieces[0, 0].GetComponent<SceneChessPiece>();
            Assert.AreEqual(bishop.Piece.Name, ChessPiece.EName.Bishop);
            yield return null;
        }
    }
}