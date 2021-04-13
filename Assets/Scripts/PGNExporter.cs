using System.Collections.Generic;
using System.Linq;
using System.Text;
public static class PGNExporter {
    // Exports a *single* game to a PGN formatted string
    // Not techniocally a PGN exporter since it doesn't add any of the game tags or whatever.
    public static string ToPGN(ChessBoard board) {
        var turn = 0;
        var sb = new StringBuilder();
        var canMove = ChessPiece.EColour.White;
        var moveStack = new Stack<(Move, ChessPiece)>(new Stack<(Move, ChessPiece)>(board.UndoStack));
        var moveList = moveStack.ToList();

        moveList.Reverse();
        board = new ChessBoard();

        foreach (var (move, capturedPiece) in moveList) {
            var isCapture = capturedPiece != null;
            if (canMove == ChessPiece.EColour.White) {
                turn++;
                sb.Append(turn);
                sb.Append(".");
            }

            var san = MoveToSAN(board, move, isCapture);
            sb.Append(san);
            if (!board.Move(move)) throw new System.Exception($"ATTEMPTED INVALID MOVE: {move}");

            if (board.State[canMove.Inverse()] == ChessBoard.BoardStatus.Check) {
                sb.Append("+");
            }


            sb.Append(" ");
            canMove = canMove.Inverse();
        }

        return sb.ToString().TrimEnd(new char[] { ' ' });
    }
    public static string MoveToSAN(ChessBoard board, Move move, bool isCapture) {
        if (move.IsCastling) {
            if (IsQueensideCastle(move)) return PGNParser.QUEENSIDE_CASTLE;
            if (IsKingSideCastle(move)) return PGNParser.KINGSIDE_CASTLE;
            else throw new System.Exception($"Invalid castle: {move}");
        }
        var (FromRow, FromColumn, ToRow, ToColumn) = move.ToCoordinates();

        var piece = board.Pieces[FromRow, FromColumn];
        var fromFile = "";
        var fromRank = "";

        // Check if the move is ambiguous
        foreach (var m in board.ValidMoves[piece.Colour]) {
            if (m.ToRow != ToRow || m.ToColumn != ToColumn) continue;
            if (m == move) continue;
            var n = board.Pieces[m.FromRow, m.FromColumn].Name;
            if (n != piece.Name) continue;

            // This move is ambiguous - try to fix that.
            if (m.FromColumn != FromColumn) fromFile = COLUMN_TO_FILE[FromColumn];
            else if (m.FromRow != FromRow) fromRank = ROW_TO_RANK[FromRow];
        }

        if (isCapture && piece.Name == ChessPiece.EName.Pawn) {
            fromFile = COLUMN_TO_FILE[FromColumn];
        }

        var toFile = COLUMN_TO_FILE[ToColumn];
        var toRank = ROW_TO_RANK[ToRow];

        var fromCoordinates = $"{fromFile}{fromRank}";
        var toCoordinates = $"{toFile}{toRank}";

        var pieceName = PIECE_NAMES[piece.Name];
        var capture = isCapture ? "x" : "";
        var promotion = move.IsPromotion() ? $"={PIECE_NAMES[move.PieceToPromoteTo]}" : "";
        var san = $"{pieceName}{fromCoordinates}{capture}{toCoordinates}{promotion}";
        return san;
    }

    private static bool IsKingSideCastle(Move move) => move.ToColumn == 6;

    private static bool IsQueensideCastle(Move move) => move.ToColumn == 2;

    static string[] ROW_TO_RANK = new string[8] {
        "8",
        "7",
        "6",
        "5",
        "4",
        "3",
        "2",
        "1"
    };
    static string[] COLUMN_TO_FILE = new string[8] {
        "a",
        "b",
        "c",
        "d",
        "e",
        "f",
        "g",
        "h"
    };


    // knight = "N", bishop = "B", rook = "R", queen = "Q", and king = "K".
    static Dictionary<ChessPiece.EName, string> PIECE_NAMES = new Dictionary<ChessPiece.EName, string> {
        { ChessPiece.EName.Knight, "N" },
        { ChessPiece.EName.Pawn, ""},
        { ChessPiece.EName.King, "K" },
        { ChessPiece.EName.Bishop, "B" },
        { ChessPiece.EName.Rook, "R" },
        { ChessPiece.EName.Queen, "Q" },
    };
}