using System.Collections.Generic;
using System.Text;
public static class PGNExporter {
    // Exports a *single* game to a PGN formatted string
    // Not techniocally a PGN exporter since it doesn't add any of the game tags or whatever.
    public static string ToPGN(ChessBoard board) {
        var turn = 1;
        var sb = new StringBuilder();
        var isWhite = true;
        var moveStack = new Stack<Move>();
        while (board.UndoStack.Count != 0) {
            var move = board.GetLastMove();
            moveStack.Push(move);
            board.Undo();
        }

        foreach (var move in moveStack) {
            if (isWhite) {
                sb.Append(turn);
                sb.Append(".");
            }

            var san = MoveToSAN(board, move);
            sb.Append(san);
            if (!board.Move(move)) throw new System.Exception($"ATTEMPTED INVALID MOVE: {move}");

            sb.Append(" ");
            isWhite = !isWhite;

        }

        return sb.ToString().TrimEnd(new char[] { ' ' });
    }
    public static string MoveToSAN(ChessBoard board, Move move) {
        var (FromRow, FromColumn, ToRow, ToColumn) = move.ToCoordinates();
        var file = COLUMN_TO_FILE[ToColumn];
        var rank = ROW_TO_RANK[ToRow];
        var coordinates = $"{file}{rank}";
        var piece = board.Pieces[FromRow, FromColumn];
        var pieceName = PIECE_NAMES[piece.Name];
        var san = $"{pieceName}{coordinates}";
        return san;
    }

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