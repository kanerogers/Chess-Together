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

            GameObject gameGameObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/Chess Board"));
            BoardInterfaceManager bi = gameGameObject.GetComponent<BoardInterfaceManager>();
            bi.enabled = false;
            var board = gameGameObject.GetComponent<SceneChessBoard>();
            board.InitializeBoard(new ChessBoard());
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
            var piece = board.Pieces[1, 0].GetComponent<SceneChessPiece>();
            Assert.IsTrue(board.Move(piece, 2, 0));

            piece = board.Pieces[2, 0].GetComponent<SceneChessPiece>();
            yield return new WaitForSeconds(board.MovementDuration + 0.1f);
            var expectedPosition = board.CoordinatesForPosition(2, 0);
            Assert.AreEqual(piece.transform.localPosition, expectedPosition);
        }
    }
}