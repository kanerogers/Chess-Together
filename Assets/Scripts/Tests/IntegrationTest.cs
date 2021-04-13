using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System;

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
            var scenePiece = sceneBoard.Pieces[fromRow, fromColumn];

            // Step two, mark it as grabbed
            scenePiece.Grabbed();

            // Step three, move it in space to where it should go
            var newPosition = sceneBoard.CoordinatesForPosition(toRow, toColumn);
            newPosition.y = 0.1f;
            scenePiece.transform.localPosition = newPosition;

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
            var pawn = sceneBoard.Pieces[4, 0];
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
            var game = 0;


            EventManager.MoveComplete += (ChessPiece.EColour justMoved) => {
                if (justMoved.IsBlack()) {
                    var whiteState = logicBoard.State[ChessPiece.EColour.White];
                    if (whiteState == ChessBoard.BoardStatus.Checkmate || whiteState == ChessBoard.BoardStatus.Stalemate) {
                        Logger.Log("Black won game ", game);
                        finished = true;
                    }

                    canMove = true;
                }
            };

            while (true) {
                while (!canMove) yield return null;
                if (finished) {
                    gameManager.Reset();
                    gameManager.StartAIGame();
                    logicBoard = gameManager.LogicBoard;
                    canMove = true;
                    continue;
                }

                canMove = false;
                Move nextMove;
                try {
                    nextMove = AIManager.GetMove(logicBoard, ChessPiece.EColour.White, AIManager.MoveType.Standard);
                    Logger.Log("Next move is ", nextMove);
                } catch (System.Exception e) {
                    var pgn = PGNExporter.ToPGN(logicBoard);
                    Logger.Log(pgn);
                    throw e;
                }

                yield return NextTurn(gameManager, nextMove);

                var blackState = logicBoard.State[ChessPiece.EColour.Black];
                if (blackState == ChessBoard.BoardStatus.Checkmate || blackState == ChessBoard.BoardStatus.Stalemate) {
                    Logger.Log("White won game ", game);
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
            ChessBoard.Inst = logicBoard;

            EventManager.MoveComplete += (ChessPiece.EColour justMoved) => {
                turn += 1;
                Logger.Log($"Just moved: {justMoved}");
                canMove = true;
            };

            while (turn != moves.Count) {
                while (!canMove) yield return null;
                canMove = false;
                var move = moves[turn];
                Assert.IsTrue(logicBoard.ValidMoves[logicBoard.CanMove].Count != 0);
                yield return NextTurn(gameManager, move);
            }
        }

        [UnityTest]
        [Timeout(100000000)]
        public IEnumerator BadGameTest() {
            yield return LoadScene();
            var gameManager = GetGameManager();
            yield return StartAIGame(gameManager);
            var sceneBoard = gameManager.SceneBoard;
            gameManager.opponentType = GameManager.OpponentType.None;

            var logicBoard = gameManager.LogicBoard;
            bool canMove = true;
            var parser = new PGNParser();
            var badPGN = "1.a4 d5 2.h3 a5 3.Na3 c6 4.Ra2 Nf6 5.d3 Kd7 6.Be3 b6 7.Bf4 Na6 8.Bh2 Nb4 9.Bc7 Qe8 10.Nf3 e5 11.Nxe5+ Ke7 12.Ra1 Ra6 13.h4 Bb7 14.Rb1 Nh5 15.Ra1 d4 16.e4 f5 17.exf5 h6 18.Ng6+ Kf7+ 19.Be5 Kg8 20.Qxh5 Ba8 21.Qe2 Rh7 22.b3 Bb7 23.f4 Qd7 24.Qg4 Bc5 25.Qh3 Ra8 26.c3 dxc3 27.Bxc3 Re8+ 28.Kd2 Re3 29.Qg4 h5 30.Qg5 Na6 31.Nc4 Re8 32.Rb1 b5 33.axb5 cxb5 34.Nxa5 Bd5 35.b4 Ba7 36.Kc1 Rc8 37.Ne7+ Kh8 38.Nxd5 Qxd5 39.Rb3 Kg8 40.Qg3 Qxf5 41.Rh3 Qf8 42.Nc4 bxc4 43.Ra3 Bf2 44.Qxf2 Rc6 45.f5 Re6 46.dxc4 Nb8 47.Ra7 Rh8 48.Qf4 Rb6 49.c5 Rc6 50.Qc4+ Kh7 51.Bd2 Rg8 52.Ra4 Qxf5 53.Bd3 Rf8 54.Ra3 Rc7 55.b5 Rd7 56.Bxf5+ Rxf5 57.Be3 Rff7 58.Qe4+ Kg8 59.Ra1 Rb7 60.Qb1 Rf1+ 61.Kc2 Rxb1 62.Rxb1 Rd7 63.Rb3 Rf7 64.c6 Kf8 65.Rg3 Re7 66.c7";
            var moves = parser.ParseSingleLine(badPGN);
            var turn = 0;
            ChessBoard.Inst = logicBoard;

            EventManager.MoveComplete += (ChessPiece.EColour justMoved) => {
                turn += 1;
                canMove = true;
            };

            while (turn != moves.Count) {
                while (!canMove) yield return null;
                canMove = false;
                var move = moves[turn];
                AIManager.GetMove(logicBoard, logicBoard.CanMove);
                Assert.IsTrue(logicBoard.ValidMoves[logicBoard.CanMove].Count != 0);
                Logger.Log("BAD_GAME", "Making move from PGN ", move);
                yield return NextTurn(gameManager, move);
            }
        }


        private IEnumerator NextTurn(GameManager gameManager, Move nextMove) {
            var logicBoard = gameManager.LogicBoard;
            var sceneBoard = gameManager.SceneBoard;
            var canMove = logicBoard.CanMove;

            Logger.Log($"{logicBoard.Turn} - Making {nextMove}..");
            var (fromRow, fromColumn, toRow, toColumn) = nextMove.ToCoordinates();
            var expectedPieceName = sceneBoard.Pieces[fromRow, fromColumn].Piece.Name;
            yield return MovePiece(sceneBoard, nextMove);

            try {
                var piece = sceneBoard.Pieces[toRow, toColumn].Piece;
                var name = nextMove.IsPromotion() ? nextMove.PieceToPromoteTo : piece.Name;

                Assert.AreEqual(expectedPieceName, name);
                Assert.AreEqual(canMove, piece.Colour);
                Assert.AreEqual(toRow, piece.Row);
                Assert.AreEqual(toColumn, piece.Column);
            } catch (Exception e) {
                throw e;
            }

        }
    }
}