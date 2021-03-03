using NUnit.Framework;

namespace Tests {
    [TestFixture]
    public class ChessBoardTest {
        [Test]
        public void TestPawn() {
            var board = new ChessBoard();
            // Can't move outside movement bounds
            Assert.IsFalse(board.Move(1, 0, 4, 0));
            Assert.IsFalse(board.Move(1, 0, 0, 0));
            Assert.IsFalse(board.Move(1, 0, 2, 1));

            // Move changes location
            Assert.IsTrue(board.Move(1, 0, 2, 0));
            Assert.IsNull(board.Pieces[1, 0]);
            {
                var pawn = board.Pieces[2, 0];
                Assert.AreEqual(pawn.Name, ChessPiece.EName.Pawn);
                Assert.AreEqual(pawn.Row, 2);
                Assert.AreEqual(pawn.Column, 0);
            }

            // Pawn can move two steps forward
            Assert.IsTrue(board.Move(1, 1, 3, 1));
            Assert.IsNull(board.Pieces[1, 1]);
            {
                var pawn = board.Pieces[3, 1];
                Assert.AreEqual(pawn.Name, ChessPiece.EName.Pawn);
                Assert.AreEqual(pawn.Row, 3);
                Assert.AreEqual(pawn.Column, 1);

            }

            // ...but only on first move.
            Assert.IsFalse(board.Move(3, 1, 5, 1));

            // And when the intervening space is unoccupied.
            Assert.IsTrue(board.Move(0, 1, 2, 2));
            Assert.IsFalse(board.Move(1, 2, 3, 2));

            // White can move "forward"
            Assert.IsTrue(board.Move(6, 0, 5, 0));
            Assert.IsNull(board.Pieces[6, 0]);
            {
                var pawn = board.Pieces[5, 0];
                Assert.AreEqual(pawn.Name, ChessPiece.EName.Pawn);
                Assert.AreEqual(pawn.Colour, ChessPiece.EColour.White);
                Assert.AreEqual(pawn.Row, 5);
                Assert.AreEqual(pawn.Column, 0);
            }

            // White Pawn moves forward again
            Assert.IsTrue(board.Move(5, 0, 4, 0));

            // Black Pawn takes White Pawn
            Assert.IsTrue(board.Move(3, 1, 4, 0));
            Assert.IsNull(board.Pieces[3, 1]);
            {
                var pawn = board.Pieces[4, 0];
                Assert.AreEqual(pawn.Name, ChessPiece.EName.Pawn);
                Assert.AreEqual(pawn.Colour, ChessPiece.EColour.Black);
                Assert.AreEqual(pawn.Row, 4);
                Assert.AreEqual(pawn.Column, 0);

            }

            // White pawn at 6,1 moves forward
            Assert.IsTrue(board.Move(6, 1, 5, 1));

            // But cannot move backwards
            Assert.IsFalse(board.Move(5, 1, 6, 1));

            // Pawn can't make illegal moves
            Assert.IsFalse(board.Move(6, 7, 4, 6));

            board = new ChessBoard();

            // Pawns can only take pieces on diagonal
            Assert.IsTrue(board.Move(6, 1, 4, 1));
            Assert.IsTrue(board.Move(4, 1, 3, 1));
            Assert.IsTrue(board.Move(3, 1, 2, 1));

        }

        [Test]
        public void TestEnPassant() {
            // En passant
            var lazy = false;

            // FIDE 3.7d
            // A pawn attacking a square crossed by an opponent’s pawn which has advanced two
            // squares in one move from its original square may capture this opponent’s pawn as
            // though the latter had been moved only one square. 
            var board = new ChessBoard();

            // White moves forward
            Assert.IsTrue(board.Move(6, 0, 4, 0, lazy));
            Assert.IsTrue(board.Move(4, 0, 3, 0, lazy));

            // Black advances two squares
            Assert.IsTrue(board.Move(1, 1, 3, 1, lazy));

            // En passant capture
            Assert.IsTrue(board.Move(3, 0, 2, 1, lazy));

            // This capture is only legal on the move following this advance..
            board = new ChessBoard();

            // White moves forward
            Assert.IsTrue(board.Move(6, 0, 4, 0, lazy));
            Assert.IsTrue(board.Move(4, 0, 3, 0, lazy));

            // Black advances two squares
            Assert.IsTrue(board.Move(1, 1, 3, 1, lazy));

            // White makes another move, invalidating en passant
            Assert.IsTrue(board.Move(6, 2, 5, 2, lazy));

            // En passant not possible.
            Assert.IsFalse(board.Move(3, 0, 2, 1, lazy));
        }

        // FIDE 3.5 "[...] the bishop [...] may not move over any intervening pieces."
        // FIDE 3.2 "The bishop may move to any square along a diagonal on which it stands."
        [Test]
        public void TestBishop() {
            var board = new ChessBoard();
            // Can't jump over a piece
            Assert.IsFalse(board.Move(0, 2, 2, 0));

            // Move a pawn so it can open
            Assert.IsTrue(board.Move(1, 3, 2, 3, true));

            // Move diagonal
            Assert.IsTrue(board.Move(0, 2, 3, 5, true));
            Assert.IsNull(board.Pieces[0, 2]);
            {
                var bishop = board.Pieces[3, 5];
                Assert.AreEqual(bishop.Name, ChessPiece.EName.Bishop);
                Assert.AreEqual(bishop.Colour, ChessPiece.EColour.Black);
                Assert.AreEqual(bishop.Row, 3);
                Assert.AreEqual(bishop.Column, 5);
            }

            // Test invalid moves
            Assert.IsFalse(board.Move(3, 5, 3, 4));
            Assert.IsFalse(board.Move(3, 5, 4, 5));
            Assert.IsFalse(board.Move(3, 5, 7, 5));
            Assert.IsFalse(board.Move(3, 5, 3, 0));

            // Take a piece
            Assert.IsTrue(board.Move(3, 5, 6, 2));
            Assert.IsNull(board.Pieces[3, 5]);
            {
                var bishop = board.Pieces[6, 2];
                Assert.AreEqual(bishop.Name, ChessPiece.EName.Bishop);
                Assert.AreEqual(bishop.Colour, ChessPiece.EColour.Black);
                Assert.AreEqual(bishop.Row, 6);
                Assert.AreEqual(bishop.Column, 2);
            }
        }

        // FIDE 3.5 "[...] the rook [...] may not move over any intervening pieces."
        // FIDE 3.3 The rook may move to any square along the file or the rank on which it stands
        [Test]
        public void TestRook() {
            var board = new ChessBoard();
            // Can't jump pieces
            Assert.IsFalse(board.Move(0, 0, 2, 0));

            // Move a pawn so it can open
            Assert.IsTrue(board.Move(1, 0, 3, 0));

            // Move along rank or file
            Assert.IsTrue(board.Move(0, 0, 1, 0));

            // Move a pawn on the other side
            Assert.IsTrue(board.Move(1, 7, 2, 7));

            // Can't jump over that row
            Assert.IsFalse(board.Move(1, 0, 1, 7));

            // Move around
            Assert.IsTrue(board.Move(1, 0, 2, 0));
            Assert.IsTrue(board.Move(2, 0, 2, 6));
            {
                var rook = board.Pieces[2, 6];
                Assert.AreEqual(rook.Name, ChessPiece.EName.Rook);
                Assert.AreEqual(rook.Colour, ChessPiece.EColour.Black);
                Assert.AreEqual(rook.Row, 2);
                Assert.AreEqual(rook.Column, 6);
            }


            // But not diagonal
            Assert.IsFalse(board.Move(2, 6, 3, 7));

            // Can't teleport
            Assert.IsFalse(board.Move(2, 6, 1, 0));
        }

        // FIDE 3.5 "[...] the queen [...] may not move over any intervening pieces."
        // FIDE 3.4 The queen may move to any square along the file, the rank or a diagonal 
        // on which it stands.
        [Test]
        public void TestQueen() {
            var board = new ChessBoard();
            Assert.IsFalse(board.Move(0, 3, 3, 0));

            // Move a pawn so it can open
            Assert.IsTrue(board.Move(1, 3, 3, 3));

            // Move along rank or file
            Assert.IsTrue(board.Move(0, 3, 1, 3));

            // Move a pawn on the other side
            Assert.IsTrue(board.Move(1, 7, 2, 7));

            // Can't jump over that row
            Assert.IsFalse(board.Move(1, 3, 1, 7));

            // Move around
            Assert.IsTrue(board.Move(1, 3, 4, 0));
            Assert.IsTrue(board.Move(4, 0, 4, 7));
            {
                var queen = board.Pieces[4, 7];
                Assert.AreEqual(queen.Name, ChessPiece.EName.Queen);
                Assert.AreEqual(queen.Colour, ChessPiece.EColour.Black);
                Assert.AreEqual(queen.Row, 4);
                Assert.AreEqual(queen.Column, 7);
            }

            // Can't teleport
            Assert.IsFalse(board.Move(4, 7, 2, 6));

            // Move White Queen now

            // Move a pawn so it can open
            Assert.IsTrue(board.Move(6, 2, 5, 2));
            Assert.IsTrue(board.Move(7, 3, 4, 0));
        }

        // FIDE 3.6 The knight may move to one of the squares nearest to that on which 
        // it stands but not on the same rank, file or diagonal
        [Test]
        public void TestKnight() {
            var board = new ChessBoard();
            // Can jump pieces
            Assert.IsTrue(board.Move(0, 1, 2, 0));

            // Can make legal moves
            Assert.IsTrue(board.Move(2, 0, 0, 1));
            Assert.IsTrue(board.Move(0, 1, 2, 2));
            Assert.IsTrue(board.Move(2, 2, 3, 4));
            Assert.IsTrue(board.Move(3, 4, 5, 3));
            {
                var knight = board.Pieces[5, 3];
                Assert.AreEqual(knight.Name, ChessPiece.EName.Knight);
                Assert.AreEqual(knight.Colour, ChessPiece.EColour.Black);
                Assert.AreEqual(knight.Row, 5);
                Assert.AreEqual(knight.Column, 3);
            }

            // Can't make illegal moves
            Assert.IsFalse(board.Move(5, 3, 5, 7));
            Assert.IsFalse(board.Move(5, 3, 2, 3));
            Assert.IsFalse(board.Move(5, 3, 3, 5));
        }

        // FIDE 3.8 a. There are two different ways of moving the king:
        // by moving to any adjoining square not attacked by one or more of the opponent’s
        // pieces
        [Test]
        public void TestKing() {
            var board = new ChessBoard();
            // Open up a pawn to test
            Assert.IsTrue(board.Move(1, 4, 2, 4));

            // Can make legal moves
            Assert.IsTrue(board.Move(0, 4, 1, 4));
            Assert.IsTrue(board.Move(1, 4, 2, 3));
            Assert.IsTrue(board.Move(2, 3, 3, 3));

            // Can't make illegal moves
            Assert.IsFalse(board.Move(3, 3, 3, 0));

            // Can't move into Check
            Assert.IsTrue(board.Move(3, 3, 4, 3));
            Assert.IsTrue(board.Move(6, 1, 5, 1));
            Assert.IsFalse(board.Move(4, 3, 4, 2));
        }

        [Test]
        public void BuildChessBoard() {
            var board = new ChessBoard();

            for (int r = 0; r < 8; r++) {
                for (int c = 0; c < 8; c++) {
                    var piece = board.Pieces[r, c];

                    if (r == 0 && c == 0) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Rook);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        Assert.AreEqual(piece.Row, r);
                        Assert.AreEqual(piece.Column, c);
                        continue;
                    }

                    if (r == 0 && c == 1) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Knight);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        Assert.AreEqual(piece.Row, r);
                        Assert.AreEqual(piece.Column, c);
                        continue;
                    }

                    if (r == 0 && c == 2) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Bishop);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 0 && c == 3) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Queen);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 0 && c == 4) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.King);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 0 && c == 5) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Bishop);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 0 && c == 6) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Knight);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 0 && c == 7) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Rook);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        continue;
                    }

                    if (r == 1) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Pawn);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.Black);
                        Assert.AreEqual(piece.Row, r);
                        Assert.AreEqual(piece.Column, c);
                        continue;
                    }

                    if (r == 6) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Pawn);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 0) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Rook);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 1) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Knight);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 2) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Bishop);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 3) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Queen);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 4) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.King);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 5) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Bishop);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 6) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Knight);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    if (r == 7 && c == 7) {
                        Assert.NotNull(piece);
                        Assert.AreEqual(piece.Name, ChessPiece.EName.Rook);
                        Assert.AreEqual(piece.Colour, ChessPiece.EColour.White);
                        continue;
                    }

                    Assert.IsNull(piece);
                }
            }
        }

        [Test]
        public void TestCheckMate() {
            {
                var board = new ChessBoard();

                // Move to checkmate
                Assert.IsTrue(board.Move(6, 4, 5, 4));
                Assert.IsTrue(board.Move(7, 5, 4, 2));
                Assert.IsTrue(board.Move(7, 3, 5, 5));
                Assert.IsTrue(board.Move(5, 5, 1, 5));
                Assert.AreEqual(board.State[ChessPiece.EColour.Black], ChessBoard.BoardStatus.Checkmate);
                Assert.AreEqual(board.State[ChessPiece.EColour.White], ChessBoard.BoardStatus.NotInCheck);
            }

            {
                var board = new ChessBoard();

                // Give the King an opening
                Assert.IsTrue(board.Move(1, 3, 2, 3));
                Assert.IsTrue(board.Move(6, 4, 5, 4));
                Assert.IsTrue(board.Move(7, 5, 4, 2));
                Assert.IsTrue(board.Move(7, 3, 5, 5));
                Assert.IsTrue(board.Move(5, 5, 1, 5));
                Assert.AreEqual(board.State[ChessPiece.EColour.Black], ChessBoard.BoardStatus.Check);
                Assert.AreEqual(board.State[ChessPiece.EColour.White], ChessBoard.BoardStatus.NotInCheck);

                // Let the King move out of check
                Assert.IsTrue(board.Move(0, 4, 1, 3));
                Assert.AreEqual(board.State[ChessPiece.EColour.Black], ChessBoard.BoardStatus.NotInCheck);
                Assert.AreEqual(board.State[ChessPiece.EColour.White], ChessBoard.BoardStatus.NotInCheck);
            }
        }

        [Test]
        public void TestDuellingKings() {
            var board = new ChessBoard(true);
            var blackKing = new King(ChessPiece.EColour.Black, 0, 0);
            var whiteKing = new King(ChessPiece.EColour.White, 2, 0);

            board.BlackKing = blackKing;
            board.WhiteKing = whiteKing;

            board.Pieces[0, 0] = blackKing;
            board.Pieces[2, 0] = whiteKing;

            // Kings cannot move next to eachother
            Assert.IsFalse(board.Move(0, 0, 1, 0));
        }

        [Test]
        public void TestCheckGuard() {
            var board = new ChessBoard();

            // Move White King to position where she can threaten Black King
            Assert.IsTrue(board.Move(6, 4, 5, 4));
            Assert.IsTrue(board.Move(7, 3, 3, 7));

            // Cannot move self into check
            Assert.IsFalse(board.Move(1, 5, 2, 5));
        }

        [Test]
        public void TestValidMoves() {
            var board = new ChessBoard();
            var colour = ChessPiece.EColour.White;
            foreach (var move in board.ValidMoves[colour]) {
                // Grab a fresh board
                var b = new ChessBoard();
                b.Move(move);
            }

        }

        [Test]
        public void TestUndoSimple() {
            var board = new ChessBoard();
            Assert.IsTrue(board.Move(1, 1, 3, 1));
            board.Undo();
            Assert.IsTrue(board.Move(1, 1, 3, 1));
        }

        [Test]
        public void TestUndoComplex() {
            var board = new ChessBoard();
            var moves = new Move[] {
                new Move(6, 4, 4, 4),
                new Move(1, 3, 2, 3),
                new Move(6, 3, 4, 3),
                new Move(0, 6, 2, 5),
                new Move(7, 1, 5, 2),
                new Move(1, 6, 2, 6),
                new Move(7, 2, 5, 4),
                new Move(0, 5, 1, 6),
                new Move(7, 3, 6, 3),
                new Move(1, 2, 2, 2),
                new Move(6, 5, 5, 5),
                new Move(1, 1, 3, 1),
                new Move(7, 6, 6, 4),
                new Move(0, 1, 1, 3),
                new Move(5, 4, 2, 7),
                new Move(1, 6, 2, 7),
                new Move(6, 3, 2, 7),
            };

            foreach (var move in moves) {
                Assert.IsTrue(board.Move(move));
            }

            foreach (var move in moves) {
                board.Undo();
            }

            foreach (var move in moves) {
                Assert.IsTrue(board.Move(move));
            }

        }

        [Test]
        public void TestCastling() {
            // FIDE 3.8a
            // "...or by ‘castling’. This is a move of the king and either rook of the same colour along
            // the player’s first rank, counting as a single move of the king and executed as
            // follows: the king is transferred from its original square two squares towards the
            // rook on its original square, then that rook is transferred to the square the king has
            // just crossed."

            // Clear board to make things a bit easier.
            var board = new ChessBoard(true);
            ChessPiece whiteKing, whiteRook, blackKing, blackRook;

            // Now add the pieces we need.
            board.CreatePiece(ChessPiece.EName.King, 7, 4, ChessPiece.EColour.White);
            board.CreatePiece(ChessPiece.EName.Rook, 7, 7, ChessPiece.EColour.White);
            board.CreatePiece(ChessPiece.EName.Rook, 7, 0, ChessPiece.EColour.White);

            board.CreatePiece(ChessPiece.EName.King, 0, 4, ChessPiece.EColour.Black);
            board.CreatePiece(ChessPiece.EName.Rook, 0, 7, ChessPiece.EColour.Black);
            board.CreatePiece(ChessPiece.EName.Rook, 0, 0, ChessPiece.EColour.Black);

            // Keep the board happy.
            board.BlackKing = (King)board.Pieces[0, 4];
            board.WhiteKing = (King)board.Pieces[7, 4];

            // Test White King -> Kingside Rook

            var whiteKingSideCastle = new Move(7, 4, 7, 7);
            Assert.IsTrue(board.Move(whiteKingSideCastle));
            whiteKing = board.Pieces[7, 6];
            Assert.IsTrue(whiteKing.Colour == ChessPiece.EColour.White);
            Assert.IsTrue(whiteKing.Name == ChessPiece.EName.King);

            whiteRook = board.Pieces[7, 5];
            Assert.IsTrue(whiteRook.Colour == ChessPiece.EColour.White);
            Assert.IsTrue(whiteRook.Name == ChessPiece.EName.Rook);
            board.Undo();

            // Make sure undo works
            whiteKing = board.Pieces[7, 4];
            Assert.AreEqual(whiteKing.Column, 4);
            Assert.AreEqual(whiteKing.Colour, ChessPiece.EColour.White);
            Assert.AreEqual(whiteKing.Name, ChessPiece.EName.King);

            whiteRook = board.Pieces[7, 7];
            Assert.AreEqual(whiteRook.Column, 7);
            Assert.IsTrue(whiteRook.Colour == ChessPiece.EColour.White);
            Assert.IsTrue(whiteRook.Name == ChessPiece.EName.Rook);

            // Test White King -> Queenside Rook

            var whiteQueenSideCastle = new Move(7, 4, 7, 0);
            Assert.IsTrue(board.Move(whiteQueenSideCastle));
            whiteKing = board.Pieces[7, 2];
            Assert.IsTrue(whiteKing.Colour == ChessPiece.EColour.White);
            Assert.IsTrue(whiteKing.Name == ChessPiece.EName.King);

            whiteRook = board.Pieces[7, 3];
            Assert.IsTrue(whiteRook.Colour == ChessPiece.EColour.White);
            Assert.IsTrue(whiteRook.Name == ChessPiece.EName.Rook);
            board.Undo();

            // Make sure undo works
            whiteKing = board.Pieces[7, 4];
            Assert.AreEqual(whiteKing.Column, 4);
            Assert.AreEqual(whiteKing.Colour, ChessPiece.EColour.White);
            Assert.AreEqual(whiteKing.Name, ChessPiece.EName.King);

            whiteRook = board.Pieces[7, 0];
            Assert.AreEqual(whiteRook.Column, 0);
            Assert.IsTrue(whiteRook.Colour == ChessPiece.EColour.White);
            Assert.IsTrue(whiteRook.Name == ChessPiece.EName.Rook);

            // Test Black King -> Kingside Rook
            var blackKingSideCastle = new Move(0, 4, 0, 7);
            Assert.IsTrue(board.Move(blackKingSideCastle));

            blackKing = board.Pieces[0, 6];
            Assert.IsTrue(blackKing.Colour == ChessPiece.EColour.Black);
            Assert.IsTrue(blackKing.Name == ChessPiece.EName.King);

            blackRook = board.Pieces[0, 5];
            Assert.IsTrue(blackRook.Colour == ChessPiece.EColour.Black);
            Assert.IsTrue(blackRook.Name == ChessPiece.EName.Rook);
            board.Undo();

            // Ensure undo works.
            blackKing = board.Pieces[0, 4];
            Assert.AreEqual(blackKing.Column, 4);
            Assert.AreEqual(blackKing.Colour, ChessPiece.EColour.Black);
            Assert.AreEqual(blackKing.Name, ChessPiece.EName.King);

            blackRook = board.Pieces[0, 7];
            Assert.AreEqual(blackRook.Column, 7);
            Assert.IsTrue(blackRook.Colour == ChessPiece.EColour.Black);
            Assert.IsTrue(blackRook.Name == ChessPiece.EName.Rook);

            // Test Black King -> Queenside Rook
            var blackQueenSideCastle = new Move(0, 4, 0, 0);
            Assert.IsTrue(board.Move(blackQueenSideCastle));

            blackKing = board.Pieces[0, 2];
            Assert.IsTrue(blackKing.Colour == ChessPiece.EColour.Black);
            Assert.IsTrue(blackKing.Name == ChessPiece.EName.King);

            blackRook = board.Pieces[0, 3];
            Assert.IsTrue(blackRook.Colour == ChessPiece.EColour.Black);
            Assert.IsTrue(blackRook.Name == ChessPiece.EName.Rook);
            board.Undo();

            blackKing = board.Pieces[0, 4];
            Assert.AreEqual(blackKing.Column, 4);
            Assert.AreEqual(blackKing.Colour, ChessPiece.EColour.Black);
            Assert.AreEqual(blackKing.Name, ChessPiece.EName.King);

            blackRook = board.Pieces[0, 0];
            Assert.AreEqual(blackRook.Column, 0);
            Assert.IsTrue(blackRook.Colour == ChessPiece.EColour.Black);
            Assert.IsTrue(blackRook.Name == ChessPiece.EName.Rook);

            // FIDE 3.8b(1)
            // "The right to castle has been lost.."
            // "(a) if the king has already moved"
            Assert.IsTrue(board.Move(0, 4, 1, 4));
            Assert.IsTrue(board.Move(1, 4, 0, 4));
            Assert.IsFalse(board.Move(blackKingSideCastle));
            Assert.IsFalse(board.Move(blackQueenSideCastle));
            board.Undo();
            board.Undo();

            // "(b) with a rook that has already moved"
            Assert.IsTrue(board.Move(0, 7, 1, 7));
            Assert.IsTrue(board.Move(1, 7, 0, 7));
            Assert.IsFalse(board.Move(blackKingSideCastle));

            // But we can still castle with the other rook.
            Assert.IsTrue(board.Move(blackQueenSideCastle));
            board.Undo();

            // FIDE 3.8b(2)
            // "Castling is prevented temporarily:
            // (a) if the square on which the king stands..
            board.CreatePiece(ChessPiece.EName.Rook, 1, 4, ChessPiece.EColour.White);
            board.UpdateBoardStatus();
            Assert.IsFalse(board.Move(blackQueenSideCastle));

            // ..or the square which it must cross..
            Assert.IsTrue(board.Move(1, 4, 1, 3));
            Assert.IsFalse(board.Move(blackQueenSideCastle));

            // ..or the square which it is to occupy..
            Assert.IsTrue(board.Move(1, 3, 1, 2));
            Assert.IsFalse(board.Move(blackQueenSideCastle));

            // ..is attacked by one or more of the the opponent's pieces"

            // Let's just double check that castling is still possible with the enemy piece gone.
            Assert.IsTrue(board.Move(1, 2, 1, 7));
            Assert.IsTrue(board.Move(blackQueenSideCastle));
            board.Undo();

            // (b) if there is any piece between the king and the rook with which castling is
            // to be effected.
            board.CreatePiece(ChessPiece.EName.Knight, 0, 1, ChessPiece.EColour.Black);
            Assert.IsFalse(board.Move(blackQueenSideCastle));
        }

        [Test]
        public void TestCastlingCorruption() {
            var blackKingSideCastle = new Move(0, 4, 0, 7);
            // Okay, now let's try castling with all the pieces on the board.
            var board = new ChessBoard();

            // Move all the pieces out of the way
            // Move a pawn to make space for the bishop.
            Assert.IsTrue(board.Move(1, 6, 2, 6));

            // Move the knight
            Assert.IsTrue(board.Move(0, 6, 2, 5));

            // Move the bishop
            Assert.IsTrue(board.Move(0, 5, 2, 7, true));

            var boardStateBefore = board.ToString();

            // Now *castle*, baby.
            Assert.IsTrue(board.Move(blackKingSideCastle, true));

            // Still there, bishop?
            var bishop = board.Pieces[2, 7];
            Assert.IsNotNull(bishop);
            board.Undo();

            // ..still actually there?
            bishop = board.Pieces[2, 7];
            Assert.IsNotNull(bishop);

            var boardStateAfter = board.ToString();
            Assert.AreEqual(boardStateBefore, boardStateAfter);
        }

        [Test]
        public void TestStalemate() {
            var board = new ChessBoard(true);

            // Board state is Black King at 0,0, White Rook at 1,3, White Bishop at 5,6, White King at 6,2, Black Pawn at 7,1,
            board.CreatePiece(ChessPiece.EName.King, 0, 0, ChessPiece.EColour.Black);
            board.CreatePiece(ChessPiece.EName.Rook, 1, 3, ChessPiece.EColour.White);
            board.CreatePiece(ChessPiece.EName.Bishop, 5, 6, ChessPiece.EColour.White);
            board.CreatePiece(ChessPiece.EName.King, 6, 2, ChessPiece.EColour.White);
            board.CreatePiece(ChessPiece.EName.Pawn, 7, 1, ChessPiece.EColour.White);
            board.UpdateBoardStatus();

            Assert.AreEqual(board.State[ChessPiece.EColour.Black], ChessBoard.BoardStatus.Stalemate);
        }

        [Test]
        public void TestPromotion() {
            var board = new ChessBoard(true);
            board.CreatePiece(ChessPiece.EName.Pawn, 6, 0, ChessPiece.EColour.Black);
            board.CreatePiece(ChessPiece.EName.Pawn, 1, 0, ChessPiece.EColour.White);
            board.CreatePiece(ChessPiece.EName.King, 7, 4, ChessPiece.EColour.White);
            board.CreatePiece(ChessPiece.EName.King, 0, 4, ChessPiece.EColour.Black);

            board.BlackKing = (King)board.Pieces[0, 4];
            board.WhiteKing = (King)board.Pieces[7, 4];

            var promotionMove = new Move(6, 0, 7, 0, ChessPiece.EName.Queen);
            board.Move(promotionMove);
            Assert.AreEqual(board.Pieces[7, 0].Name, ChessPiece.EName.Queen);
            Assert.IsNull(board.Pieces[6, 0]);
            Assert.AreEqual(board.Turn, 1);

            board.Undo();
            Assert.IsNull(board.Pieces[7, 0]);
            Assert.AreEqual(board.Pieces[6, 0].Name, ChessPiece.EName.Pawn);
            Assert.AreEqual(board.Pieces[6, 0].Row, 6);
            Assert.AreEqual(board.Pieces[6, 0].Column, 0);
            Assert.AreEqual(board.Turn, 0);

            // Now do it for white!
            promotionMove.FromRow = 1;
            promotionMove.ToRow = 0;

            board.Move(promotionMove);
            Assert.AreEqual(board.Pieces[0, 0].Name, ChessPiece.EName.Queen);
            Assert.IsNull(board.Pieces[1, 0]);
            Assert.AreEqual(board.Turn, 1);

            board.Undo();
            Assert.IsNull(board.Pieces[0, 0]);
            Assert.AreEqual(board.Pieces[1, 0].Name, ChessPiece.EName.Pawn);
            Assert.AreEqual(board.Pieces[1, 0].Row, 1);
            Assert.AreEqual(board.Pieces[1, 0].Column, 0);
            Assert.AreEqual(board.Turn, 0);
            board.UpdateBoardStatus();

            bool hasBishopPromotion = false;
            bool hasQueenPromotion = false;
            bool hasKnightPromotion = false;
            bool hasRookPromotion = false;

            // Make sure that promotions are registered as valid moves.
            foreach (var move in board.ValidMoves[ChessPiece.EColour.Black]) {
                Logger.Log("TEST", move.ToString());
                if (move.PieceToPromoteTo == ChessPiece.EName.Rook && move.ToRow == 7 && move.ToColumn == 0) hasRookPromotion = true;
                if (move.PieceToPromoteTo == ChessPiece.EName.Knight && move.ToRow == 7 && move.ToColumn == 0) hasKnightPromotion = true;
                if (move.PieceToPromoteTo == ChessPiece.EName.Queen && move.ToRow == 7 && move.ToColumn == 0) hasQueenPromotion = true;
                if (move.PieceToPromoteTo == ChessPiece.EName.Bishop && move.ToRow == 7 && move.ToColumn == 0) hasBishopPromotion = true;
            }

            Assert.IsTrue(hasBishopPromotion);
            Assert.IsTrue(hasQueenPromotion);
            Assert.IsTrue(hasKnightPromotion);
            Assert.IsTrue(hasRookPromotion);

            hasBishopPromotion = false;
            hasQueenPromotion = false;
            hasKnightPromotion = false;
            hasRookPromotion = false;

            foreach (var move in board.ValidMoves[ChessPiece.EColour.White]) {
                Logger.Log("TEST", move.ToString());
                if (move.PieceToPromoteTo == ChessPiece.EName.Rook && move.ToRow == 0 && move.ToColumn == 0) hasRookPromotion = true;
                if (move.PieceToPromoteTo == ChessPiece.EName.Knight && move.ToRow == 0 && move.ToColumn == 0) hasKnightPromotion = true;
                if (move.PieceToPromoteTo == ChessPiece.EName.Queen && move.ToRow == 0 && move.ToColumn == 0) hasQueenPromotion = true;
                if (move.PieceToPromoteTo == ChessPiece.EName.Bishop && move.ToRow == 0 && move.ToColumn == 0) hasBishopPromotion = true;
            }

            Assert.IsTrue(hasBishopPromotion);
            Assert.IsTrue(hasQueenPromotion);
            Assert.IsTrue(hasKnightPromotion);
            Assert.IsTrue(hasRookPromotion);
        }

    }
}