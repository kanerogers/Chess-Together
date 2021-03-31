using NUnit.Framework;
using System;
using System.IO;

namespace Tests {
    public class EnPassantBugTest {
        const char WHITE = 'W';
        const char BLACK = 'B';
        [Test]
        public void BoardStateTest() {
            var board = GetBoard();
            AIManager.GetMove(board, ChessPiece.EColour.White, AIManager.MoveType.Standard);
            AIManager.GetMove(board, ChessPiece.EColour.Black, AIManager.MoveType.Standard);
            Assert.Pass("Didn't crash!");
        }

        [Test]
        public void PGNTest() {
            var path = "Assets\\Scripts\\Tests\\en_passant_game.pgn";
            var p = new PGNParser(path);
            var games = p.Parse(1);
            var moves = games[0];
            var board = new ChessBoard();
            var canMove = ChessPiece.EColour.White;
            foreach (var move in moves) {
                // Make move
                board.Move(move);

                // Undo it
                // board.Undo();

                // Make move
                // board.Move(move);

                // Make the AI think
                if (board.Turn == 44) Logger.AT_CORRECT_TURN = true;
                try {
                    AIManager.GetMove(board, canMove, AIManager.MoveType.Standard);
                } catch (Exception e) {
                    Logger.SPECIAL_DEBUG = false;
                    Logger.Log("EPBUG", "Caught bug - can move is", canMove, "and turn is", board.Turn);
                    var pgn = PGNExporter.ToPGN(board);
                    Logger.Log("EPBUG PGN", pgn);
                    Logger.Log("EPBUG", e.ToString());
                    Logger.Log(e.ToString());
                    throw e;
                }

                try {
                    var aiMove = AIManager.GetMove(board, canMove.Inverse(), AIManager.MoveType.Standard);
                    // Play an AI move
                    board.Move(aiMove);
                } catch (Exception e) {
                    Logger.Log("EPBUG", "Caught bug - can move is", canMove, "and turn is", board.Turn);
                    var pgn = PGNExporter.ToPGN(board);
                    Logger.Log("EPBUG PGN", pgn);
                    Logger.Log("EPBUG", e.ToString());
                    throw e;
                }


                // Undo it
                board.Undo();

                canMove = canMove.Inverse();
            }
        }

        ChessBoard GetBoard() {
            var path = "Assets\\Scripts\\Tests\\en_passant_board.txt";
            var boardString = File.ReadAllLines(path);
            var board = new ChessBoard(true);
            ParseBoardString(boardString[0], board);
            board.UpdateBoardStatus();
            return board;
        }

        private void ParseBoardString(string boardString, ChessBoard board) {
            var pieces = new ChessPiece[8, 8];

            int row = -1;
            int column = -1;
            var colour = ChessPiece.EColour.Black;
            var name = ChessPiece.EName.None;
            var state = State.InRow;
            var i = 0;

            while (true) {
                if (i >= boardString.Length) break;

                var c = boardString[i];
                switch (state) {
                    case State.InRow:
                        row = (int)Char.GetNumericValue(c);
                        break;
                    case State.InColumn:
                        column = (int)Char.GetNumericValue(c);
                        break;
                    case State.InColour:
                        if (c == WHITE) colour = ChessPiece.EColour.White;
                        if (c == BLACK) colour = ChessPiece.EColour.Black;
                        break;
                    case State.InName:
                        if (c == 'P') name = ChessPiece.EName.Pawn;
                        if (c == 'B') name = ChessPiece.EName.Bishop;
                        if (c == 'Q') name = ChessPiece.EName.Queen;
                        if (c == 'R') name = ChessPiece.EName.Rook;
                        if (c == 'K') {
                            var nc = boardString[i + 1];
                            if (nc == 'i') name = ChessPiece.EName.King;
                            if (nc == 'n') name = ChessPiece.EName.Knight;
                        }

                        board.CreatePiece(name, row, column, colour);
                        break;
                        // case State.LookingForRow:
                        //     break;
                        // case State.LookingForColumn:
                        //     if (c == COMMA) continue;
                        //     break;
                        // case State.LookingForName:
                        //     break;
                        // case State.LookingForColour:
                        //     break;
                }

                i += Seek(state, colour, name);
                state = NextState(state);
            }

        }

        private int Seek(State state, ChessPiece.EColour colour, ChessPiece.EName name) {
            var padding = 10;
            if (state == State.InRow) return 3;
            if (state == State.InColumn) return 1;
            if (state == State.InColour) return 6;
            if (state == State.InName) {
                if (name == ChessPiece.EName.Pawn) return 3 + padding;
                if (name == ChessPiece.EName.Knight) return 5 + padding;
                if (name == ChessPiece.EName.King) return 3 + padding;
                if (name == ChessPiece.EName.Queen) return 4 + padding;
                if (name == ChessPiece.EName.Rook) return 3 + padding;
                if (name == ChessPiece.EName.Bishop) return 5 + padding;
            }

            throw new Exception("Invalid state");
        }

        private State NextState(State state) {
            if (state == State.InRow) return State.InColumn;
            if (state == State.InColumn) return State.InColour;
            if (state == State.InColour) return State.InName;
            if (state == State.InName) return State.InRow;
            // if (state == State.InRow) return State.LookingForColumn;
            // if (state == State.InColumn) return State.LookingForName;
            // if (state == State.InName) return State.LookingForColour;
            // if (state == State.InColour) return State.LookingForRow;
            throw new System.Exception("Invalid state!");
        }

        enum State {
            InRow,
            InColumn,
            InColour,
            InName,
            // LookingForRow,
            // LookingForColumn,
            // LookingForColour,
            // LookingForName,
        }
    }
}