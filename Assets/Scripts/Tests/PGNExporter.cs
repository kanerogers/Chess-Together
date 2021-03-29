using System.Collections.Generic;
using System.Text;
public static class PGNExporter {
    // Exports a *single* game to a PGN formatted string
    // Not techniocally a PGN exporter since it doesn't add any of the game tags or whatever.
    public static string ToPGN(ChessBoard board) {
        var turn = 0;
        var sb = new StringBuilder();
        var isWhite = true;
        var moveStack = new Stack<(Move, bool)>();

        while (board.UndoStack.Count != 0) {
            var (move, capturedPiece) = board.UndoStack.Peek();
            var isCapture = capturedPiece != null;
            moveStack.Push((move, isCapture));
            Logger.Log("PGN", "Undoing", move);
            board.Undo();
        }

        foreach (var (move, isCapture) in moveStack) {
            if (isWhite) {
                turn++;
                sb.Append(turn);
                sb.Append(".");
            }

            var san = MoveToSAN(board, move, isCapture);
            sb.Append(san);
            if (!board.Move(move)) throw new System.Exception($"ATTEMPTED INVALID MOVE: {move}");

            sb.Append(" ");
            isWhite = !isWhite;
        }

        return sb.ToString().TrimEnd(new char[] { ' ' });
    }
    public static string MoveToSAN(ChessBoard board, Move move, bool isCapture) {
        if (move.IsCastling) {
            if (IsQueensideCastle(move)) return PGNParser.QUEENSIDE_CASTLE;
            if (IsKingSideCastle(move)) return PGNParser.KINGSIDE_CASTLE;
            else throw new System.Exception($"Invalid castle: {move}");
        }
        Logger.Log("PGN", "Parsing", move);
        var (FromRow, FromColumn, ToRow, ToColumn) = move.ToCoordinates();

        var piece = board.Pieces[FromRow, FromColumn];
        var fromFile = "";
        var fromRank = "";
        if (isCapture && piece.Name == ChessPiece.EName.Pawn) {
            fromFile = COLUMN_TO_FILE[FromColumn];
        }

        var toFile = COLUMN_TO_FILE[ToColumn];
        var toRank = ROW_TO_RANK[ToRow];
        var fromCoordinates = $"{fromFile}{fromRank}";
        var toCoordinates = $"{toFile}{toRank}";
        var pieceName = PIECE_NAMES[piece.Name];
        var capture = isCapture ? "x" : "";
        var san = $"{pieceName}{fromCoordinates}{capture}{toCoordinates}";
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