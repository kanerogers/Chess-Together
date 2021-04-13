using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System;
using System.Collections.Generic;

namespace IntegrationTests {
    public class SceneBoardTests {
        bool poolCreated, listenerAdded = false;
        GameObject poolManager;
        public SceneChessBoard GetBoard() {
            // Create pool manager
            if (!poolCreated) {
                poolManager = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/Pool Manager"));
                poolCreated = true;
            }


            // Create Game Manager
            GameManager gameManager = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Prefabs/Game Manager")).GetComponent<GameManager>();
            gameManager.opponentType = GameManager.OpponentType.None;
            if (!listenerAdded) {
                var listener = gameManager.gameObject.AddComponent<AudioListener>();
                AudioListener.volume = 0;
                listenerAdded = true;
            }

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
                    SceneChessPiece sp = board.Pieces[r, c];

                    if (r == 0 && c == 0) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Rook);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        Assert.AreEqual(piece.Row, r);
                        Assert.AreEqual(piece.Column, c);
                        continue;
                    }

                    if (r == 0 && c == 1) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Knight);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        Assert.AreEqual(piece.Row, r);
                        Assert.AreEqual(piece.Column, c);
                        continue;
                    }

                    if (r == 0 && c == 2) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Bishop);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 0 && c == 3) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Queen);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 0 && c == 4) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.King);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 0 && c == 5) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Bishop);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 0 && c == 6) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Knight);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 0 && c == 7) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Rook);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 1) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Pawn);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        Assert.AreEqual(piece.Row, r);
                        Assert.AreEqual(piece.Column, c);
                        continue;
                    }

                    if (r == 6) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Pawn);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 0) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Rook);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 1) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Knight);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 2) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Bishop);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 3) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Queen);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 4) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.King);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 5) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Bishop);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 6) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Knight);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 7) {
                        var piece = sp.Piece;
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Rook);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    Assert.IsNull(sp);
                }
            }
        }
        [UnityTest]
        public IEnumerator TestMovement() {
            var board = GetBoard();
            var piece = board.Pieces[6, 6];
            Assert.IsTrue(board.Move(piece, 4, 6));

            piece = board.Pieces[4, 6];
            yield return new WaitForSeconds(board.MovementDuration + 0.1f);
            var expectedPosition = board.CoordinatesForPosition(4, 6);
            Assert.AreEqual(expectedPosition, piece.transform.localPosition);
        }

        [UnityTest]
        public IEnumerator TestCastling() {

            var board = GetBoard();
            int turnCompleteCount = 0;
            var moves = new Move[] {
                new Move(6, 6, 4, 6),
                new Move(1, 6, 3, 6),
                new Move(7, 6, 5, 7),
                new Move(0, 6, 2, 7),
                new Move(7, 5, 6, 6),
                new Move(0, 5, 1, 6),
                new Move(7, 4, 7, 6),
                new Move(0, 4, 0, 6),
            };

            EventManager.MoveComplete += (ChessPiece.EColour _) => { turnCompleteCount += 1; };

            foreach (var move in moves) {
                board.Move(move);
                var (_, _, toRow, toColumn) = move.ToCoordinates();
                yield return new WaitForSeconds(board.MovementDuration + 0.1f);
                var piece = board.Pieces[toRow, toColumn];
                var expectedPosition = board.CoordinatesForPosition(toRow, toColumn);
                Assert.AreEqual(expectedPosition, piece.transform.localPosition);
                if (move.IsCastling) {
                    var expectedKingColumn = 6;
                    var king = board.Pieces[toRow, expectedKingColumn];
                    var expectedKingPosition = board.CoordinatesForPosition(toRow, expectedKingColumn);
                    Assert.AreEqual(expectedPosition, piece.transform.localPosition);
                    Assert.AreEqual(ChessPiece.EName.King, king.Piece.Name);

                    var expectedRookColumn = 5;
                    var rook = board.Pieces[toRow, expectedRookColumn];
                    var expectedRookPosition = board.CoordinatesForPosition(toRow, expectedRookColumn);
                    Assert.AreEqual(expectedPosition, piece.transform.localPosition);
                    Assert.AreEqual(ChessPiece.EName.Rook, rook.Piece.Name);
                }
            }

            Assert.AreEqual(8, turnCompleteCount);
        }

        [UnityTest]
        public IEnumerator TestDisappearingKing() {
            var board = GetBoard();
            var badPGN = "1.g4 d5 2.Nf3 d4 3.Bh3 Qd5 4.O-O Qc4 5.Kg2 Nh6 6.Rg1 Bd7 7.Rf1 Bc8 8.Re1";
            var moves = new PGNParser().ParseSingleLine(badPGN);

            foreach (var move in moves) {
                board.Move(move);
                var (_, _, toRow, toColumn) = move.ToCoordinates();
                yield return new WaitForSeconds(board.MovementDuration + 0.1f);
                var piece = board.Pieces[toRow, toColumn];
                var expectedPosition = board.CoordinatesForPosition(toRow, toColumn);
                Assert.AreEqual(expectedPosition, piece.transform.localPosition);
            }

            yield return null;
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

            var pawn = sceneBoard.Pieces[1, 0];
            sceneBoard.Move(pawn, 0, 0);

            Assert.AreEqual(bi.currentState, BoardInterfaceManager.State.ChoosingPieceToPromoteTo);
            bi.LeftButtonPressed();
            bi.RightButtonPressed();

            yield return new WaitForSeconds(sceneBoard.MovementDuration + 0.1f);
            var bishop = sceneBoard.Pieces[0, 0];
            Assert.AreEqual(bishop.Piece.Name, ChessPiece.EName.Bishop);
            yield return null;
        }

        [UnityTest]
        [Timeout(100000000)]
        public IEnumerator TestAIGamesForeverUntilABugIsFoundGoddamnit() {
            var board = GetBoard();
            var logicBoard = board.LogicBoard;
            var game = 0;

            while (true) {
                var canMove = logicBoard.CanMove;
                var move = AIManager.GetMove(logicBoard, canMove);
                var pgn = PGNExporter.ToPGN(logicBoard);

                try {
                    Assert.IsTrue(board.Move(move), $"Move {move} was invalid! PGN was {pgn}");
                } catch (ChessException e) {
                    Logger.LogError(e.ToString());
                    Assert.Fail($"Move {move} was invalid: {e.Message} PGN was {pgn}");
                }

                pgn = PGNExporter.ToPGN(logicBoard);

                var (_, _, toRow, toColumn) = move.ToCoordinates();
                yield return new WaitForSeconds(board.MovementDuration + 0.1f);

                var piece = board.Pieces[toRow, toColumn];
                Assert.NotNull(piece, $"Move {move} did not move the piece correctly! PGN was {pgn}");
                var expectedPosition = board.CoordinatesForPosition(toRow, toColumn);
                var actualPosition = piece.transform.localPosition;
                Assert.AreEqual(expectedPosition, actualPosition, $"Move {move} did not update the position correctly! PGN was {pgn}");
                var p = piece.Piece;
                Assert.AreEqual(toRow, p.Row, $"Move {move} did not update the row correctly! PGN was {pgn}");
                Assert.AreEqual(toColumn, p.Column, $"Move {move} did not update the column correctly! PGN was {pgn}");

                Logger.Log($"GAME_{game}_PGN", pgn);

                canMove = logicBoard.CanMove;
                var state = logicBoard.State[canMove];
                if (state == ChessBoard.BoardStatus.Checkmate || state == ChessBoard.BoardStatus.Stalemate || logicBoard.Turn > 200) {
                    break;
                }
            }
        }

        [UnityTest]
        [Timeout(1000000000)]
        public IEnumerator TestPGNDatabase() {
            var parser = new PGNParser("Assets\\Scripts\\Tests\\test.pgn");
            var games = parser.Parse(100);
            // Assert.AreEqual(3254, games.Count);
            var gameIndex = 0;
            var board = GetBoard();

            foreach (var moves in games) {
                Logger.Log($"Playing game {gameIndex}..");
                yield return PlayThroughMoves(moves, board);
                Logger.Log($"Game {gameIndex} done");
                var logicBoard = new ChessBoard();
                board.ReinitializeBoard(logicBoard);
                gameIndex++;
            }
        }

        [UnityTest]
        public IEnumerator InvalidMovesTest() {
            var badPGN = "1.a4 d5 2.h3 a5 3.Na3 c6 4.Ra2 Nf6 5.d3 Kd7 6.Be3 b6 7.Bf4 Na6 8.Bh2 Nb4 9.Bc7 Qe8 10.Nf3 e5 11.Nxe5+ Ke7 12.Ra1 Ra6 13.h4 Bb7 14.Rb1 Nh5 15.Ra1 d4 16.e4 f5 17.exf5 h6 18.Ng6+ Kf7+ 19.Be5 Kg8 20.Qxh5 Ba8 21.Qe2 Rh7 22.b3 Bb7 23.f4 Qd7 24.Qg4 Bc5 25.Qh3 Ra8 26.c3 dxc3 27.Bxc3 Re8+ 28.Kd2 Re3 29.Qg4 h5 30.Qg5 Na6 31.Nc4 Re8 32.Rb1 b5 33.axb5 cxb5 34.Nxa5 Bd5 35.b4 Ba7 36.Kc1 Rc8 37.Ne7+ Kh8 38.Nxd5 Qxd5 39.Rb3 Kg8 40.Qg3 Qxf5 41.Rh3 Qf8 42.Nc4 bxc4 43.Ra3 Bf2 44.Qxf2 Rc6 45.f5 Re6 46.dxc4 Nb8 47.Ra7 Rh8 48.Qf4 Rb6 49.c5 Rc6 50.Qc4+ Kh7 51.Bd2 Rg8 52.Ra4 Qxf5 53.Bd3 Rf8 54.Ra3 Rc7 55.b5 Rd7 56.Bxf5+ Rxf5 57.Be3 Rff7 58.Qe4+ Kg8 59.Ra1 Rb7 60.Qb1 Rf1+ 61.Kc2 Rxb1 62.Rxb1 Rd7 63.Rb3 Rf7 64.c6 Kf8 65.Rg3 Re7 66.c7";
            var parser = new PGNParser();
            var moves = parser.ParseSingleLine(badPGN);
            yield return PlayThroughMoves(moves);

        }

        [UnityTest]
        public IEnumerator InvalidCastlingTest() {
            var badPGN = "1.a4 d5 2.h3 Kd7 3.Nc3 c6 4.a5 f6 5.g3 Ke8 6.h4 Be6 7.e3 Qc7 8.Be2 Kd7 9.Ra2 Qe5 10.Na4 Qe4 11.Nf3 d4 12.Ra3 Na6 13.exd4 Bf5 14.c4 Nh6 15.O-O ";
            var parser = new PGNParser();
            var moves = parser.ParseSingleLine(badPGN);
            moves.Add(new Move(3, 5, 4, 6));
            yield return PlayThroughMoves(moves);
        }

        [UnityTest]
        public IEnumerator EnPassantTest() {
            var badPGN = "1.Nf3 d6 2.h3 b5 3.g4 e5 4.c3 Bb7 5.Bg2 f6 6.Kf1 e4 7.Nd4 a6 8.Ne6 Qd7 9.Nd4 c5 10.Nb3 c4 11.Na5 Bc8 12.Bxe4 Ra7 13.d4 cxd3";
            var parser = new PGNParser();
            var board = GetBoard();
            var moves = parser.ParseSingleLine(badPGN);
            yield return PlayThroughMoves(moves, board);
            var pieceTakenByEnPassant = board.Pieces[4, 3];
            Assert.IsNull(pieceTakenByEnPassant);
        }

        [UnityTest]
        public IEnumerator PromotionNotWorkingTest() {
            var badPGN = "1.Nf3 Nc6 2.d4 h6 3.g3 b5 4.Bg2 Rh7 5.Nfd2 d5 6.Nf1 h5 7.Qd3 g6 8.a3 Bb7 9.Ra2 Kd7 10.Bxd5 Ne5 11.dxe5 Ke8 12.c4 Qb8 13.Qd1 c6 14.Bf3 bxc4 15.Bf4 Qc7 16.e3 Rd8 17.Qe2 Rd3 18.b3 f5 19.bxc4 Qd8 20.Bd5 Rb3 21.Nbd2 cxd5 22.c5 Qa5 23.c6 Bxc6 24.Rc2 Ba8 25.Ra2 Rb6 26.Kd1 Re6 27.Nb3 Qb6 28.Na1 Kd7 29.Nc2 Bb7 30.Nd2 a6 31.Kc1 Ke8 32.Qd3 Rg7 33.Nf3 Qd8 34.Qb3 Ba8 35.Nb4 Qb6 36.Nh4 a5 37.Kb2 a4 38.Qc3 d4 39.Qxd4 Bxh1 40.Qd2 Qb5 41.f3 Qb6 42.Qc3 Qd8 43.Nd3 Rb6+ 44.Ka1 Rb3 45.Qc6+ Kf7 46.Qc4+ e6 47.Nf2 Bxa3 48.Nxh1 Qd1+ 49.Qc1 Rd3 50.Qb1 Nf6 51.exf6 g5 52.fxg7 g4 53.fxg4 Kf6";
            var parser = new PGNParser();
            var board = GetBoard();
            var moves = parser.ParseSingleLine(badPGN);
            var promotionMove = new Move(1, 6, 0, 6);
            promotionMove.PieceToPromoteTo = ChessPiece.EName.Queen;
            moves.Add(promotionMove);
            yield return PlayThroughMoves(moves, board);
        }

        [UnityTest]
        public IEnumerator PieceNotBeingUpdatedTest() {
            var badPGN = "1.d3 h6 2.e3 h5 3.Nf3 h4 4.d4 Rh6 5.e4 Rh7 6.Ng5 Rh8 7.f3 f6 8.Nh3 a6 9.c4 g5 10.e5 g4 11.fxg4 b6 12.Qf3 Nc6 13.Nd2 fxe5 14.Ng1 exd4 15.b4 Bg7 16.a4 Ra7 17.Rb1 Bb7 18.Qf4 Qb8 19.Qf5 d6 20.Qg6+ Kd8 21.Qxg7 Rh6 22.Qxg8+ Kd7 23.Qg7 Re6+ 24.Kf2 Qc8 25.Ba3 Nd8 26.Qxd4 Ra8 27.Qg7 c6 28.Qd4 Qc7 29.Ne4 Bc8 30.Rb2 Rh6 31.Qg7 Re6 32.Kf3 Qb7 33.Qf8 Qa7 34.Bd3 d5 35.cxd5 cxd5 36.Ng5 Re5 37.Qg8 Bb7 38.Rf2 d4+ 39.Kf4 Kd6 40.b5+ Rc5 41.bxa6 Nf7 42.Qxf7 Bc8 43.Qg6+ Kd7 44.Bf5+ e6 45.Bxe6+ Kc7 46.Qh7+ Bd7 47.Bxc5 Rd8 48.Qxh4 Bxa4 49.Be7 Ra8 50.Bc4 Kc8 51.Bd6 Kd7 52.Be5 Rf8+ 53.Ke4 Rc8 54.Be2 d3 55.Bxd3 Ke8 56.Bh8 Kd8 57.Rf5 Rb8 58.Kf3 Qa8+ 59.Ke3 Qxg2 60.a7 Ra8 61.Rf3 Bc6 62.Be4 Bxe4 63.Kxe4 Rc8 64.a8=B Qc2+ 65.Ke5 Rb8 66.Rf2 Qg6 67.Rf7 Qd3 68.Bf3";
            var parser = new PGNParser();
            var board = GetBoard();
            var moves = parser.ParseSingleLine(badPGN);
            yield return PlayThroughMoves(moves, board);
        }

        [UnityTest]
        public IEnumerator RemovePieceOfSameColourTest() {
            var badPGN = "1.Na3 a6 2.c4 c5 3.g4 g5 4.d3 e5 5.Nf3 Qf6 6.Nd2 Qc6 7.e4 f5 8.gxf5 Nf6 9.Nab1 g4 10.Qc2 Qd6 11.b3 Qd4 12.Nc3 Nxe4 13.dxe4 Ke7 14.Bg2 Bh6 15.f4 Bxf4 16.a4 Rf8 17.Kf1 Qd6 18.Qb1 Bxd2 19.Nd5+ Qxd5 20.cxd5 Ba5 21.Qb2 Kd6 22.Kf2 Re8 23.Kg3 Rg8 24.Qa2 Bc3 25.Rb1 Bb4 26.a5 Kc7 27.Ra1 h5 28.Kh4 Bc3 29.Rb1 Rh8 30.f6 Rh7 31.Kg5 Bb4 32.Kg6 b6 33.axb6+ Kb7 34.f7 Rh8 35.f8=R Rxf8 36.Kg7 Rd8 37.Bg5 Re8 38.Kf7";
            var parser = new PGNParser();
            var board = GetBoard();
            var moves = parser.ParseSingleLine(badPGN);
            yield return PlayThroughMoves(moves, board);
        }

        IEnumerator PlayThroughMoves(List<Move> moves, SceneChessBoard board = null) {
            if (board == null) board = GetBoard();

            var logicBoard = board.LogicBoard;
            board.ReinitializeBoard(logicBoard);
            ChessBoard.Inst = logicBoard;
            var i = 0;

            foreach (var move in moves) {
                AIManager.GetMove(logicBoard, logicBoard.CanMove);
                var pgn = PGNExporter.ToPGN(logicBoard);

                try {
                    Assert.IsTrue(board.Move(move), $"Move {move} at index {i} was invalid! PGN was {pgn}");
                } catch (ChessException e) {
                    Logger.LogError(e.ToString());
                    Assert.Fail($"Move {move} at index {i} was invalid: {e.Message} PGN was {pgn}");
                }

                var (_, _, toRow, toColumn) = move.ToCoordinates();
                yield return new WaitForSeconds(board.MovementDuration + 0.1f);

                pgn = PGNExporter.ToPGN(logicBoard);

                var piece = board.Pieces[toRow, toColumn];
                Assert.NotNull(piece, $"Move {move} did not move the piece correctly! PGN was {pgn}");

                var expectedPosition = board.CoordinatesForPosition(toRow, toColumn);
                var actualPosition = piece.transform.localPosition;
                Assert.AreEqual(expectedPosition, actualPosition, $"Move {move} did not update the position correctly! Expected {expectedPosition} but was {actualPosition}! PGN was {pgn}");

                var p = piece.Piece;
                Assert.AreEqual(toRow, p.Row, $"Move {move} did not update the row correctly! Expected {toRow} but was {p.Row}! PGN was {pgn}");
                Assert.AreEqual(toColumn, p.Column, $"Move {move} did not update the column correctly! Expected {toColumn} but was {p.Column}! PGN was {pgn}");

                Logger.Log($"[{i}] PGN", pgn);
                i++;
            }

        }
    }
}