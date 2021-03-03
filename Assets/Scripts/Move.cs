using System;
using System.Collections.Generic;

public class Move : IEquatable<Move> {
    public int Sequence;
    public int FromRow;
    public int FromColumn;
    public int ToRow;
    public int ToColumn;
    public int Score;
    public string Player;
    public bool isCastling;
    public bool firstMoved;
    public ChessPiece.EName PieceToPromoteTo;
    public PreviousEnPassantState previousEnPassantState;

    public Move(int fromRow, int fromColumn, int toRow, int toColumn) {
        FromRow = fromRow;
        FromColumn = fromColumn;
        ToRow = toRow;
        ToColumn = toColumn;
    }

    public Move(int fromRow, int fromColumn, int toRow, int toColumn, ChessPiece.EName pieceToPromoteTo) {
        FromRow = fromRow;
        FromColumn = fromColumn;
        ToRow = toRow;
        ToColumn = toColumn;
        PieceToPromoteTo = pieceToPromoteTo;
    }

    public Move(int fromRow, int fromColumn, int toRow, int toColumn, int score) {
        FromRow = fromRow;
        FromColumn = fromColumn;
        ToRow = toRow;
        ToColumn = toColumn;
        Score = score;
    }

    public Move(int fromRow, int fromColumn, int toRow, int toColumn, int sequence, string player) {
        FromRow = fromRow;
        FromColumn = fromColumn;
        ToRow = toRow;
        ToColumn = toColumn;
        Sequence = sequence;
        Player = player;
    }

    public override string ToString() {
        if (PieceToPromoteTo == 0) {
            return $"Sequence {Sequence}: {FromRow},{FromColumn} to {ToRow},{ToColumn}. Score: {Score}";
        } else {
            return $"Sequence {Sequence}: {FromRow},{FromColumn} to {ToRow},{ToColumn}. Score: {Score}, Promote to: {PieceToPromoteTo}";
        }
    }

    internal (int, int, int, int) ToCoordinates() {
        return (FromRow, FromColumn, ToRow, ToColumn);
    }

    public Move(object obj) {
        var source = (IDictionary<string, object>)obj;

        FromRow = (int)(long)source["FromRow"];
        FromColumn = (int)(long)source["FromColumn"];
        ToRow = (int)(long)source["ToRow"];
        ToColumn = (int)(long)source["ToColumn"];
        Player = (string)source["Player"];
        Sequence = (int)(long)source["Sequence"];
    }

    public Dictionary<string, object> ToDictionary() {
        var d = new Dictionary<string, object>();
        d.Add("FromRow", FromRow);
        d.Add("FromColumn", FromColumn);
        d.Add("ToRow", ToRow);
        d.Add("ToColumn", ToColumn);
        d.Add("Player", Player);
        d.Add("Sequence", Sequence);
        return d;
    }

    public bool Equals(Move move) {
        if (move == null) return false;
        return (move.ToRow == ToRow && move.ToColumn == ToColumn && move.FromRow == FromRow && move.FromColumn == FromColumn);
    }

}