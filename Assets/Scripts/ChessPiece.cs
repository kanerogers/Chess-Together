using System;

public abstract class ChessPiece {
    public EColour Colour;
    public EName Name;
    public int Row;
    public int Column;
    public bool HasMoved;

    public virtual bool CheckMove(ChessPiece[,] pieces, Move move) {
        var (_, _, toRow, toColumn) = move.ToCoordinates();

        if (toRow == Row && toColumn == Column) {
            Logger.Log("Moves", $"Move {move} is invalid. Can't move to same square");
            return false;
        }

        if (toRow > 7 || toRow < 0) {
            return false;
        }
        if (toColumn > 7 || toColumn < 0) {
            Logger.Log("Moves", $"Move {move} is invalid. Invalid column {toColumn}");
            return false;
        }

        ChessPiece piece = pieces[toRow, toColumn];

        if (piece != null) {
            // FIDE 3.1 It is not permitted to move a piece to a square occupied by a piece of the same colour.
            if (piece.Colour == Colour) {
                Logger.Log("Moves", $"Move {move} is invalid. {piece} is the same colour as {this}");
                return false;
            }
        }

        return true;
    }

    public abstract int GetScore();

    public bool HasPiecesOnInterveningSquares(ChessPiece[,] pieces, int toRow, int toColumn) {
        int rowDelta;
        int colDelta;

        if (toRow > Row) rowDelta = 1;
        else if (toRow < Row) rowDelta = -1;
        else rowDelta = 0;

        if (toColumn > Column) colDelta = 1;
        else if (toColumn < Column) colDelta = -1;
        else colDelta = 0;

        int row = Row + rowDelta;
        int column = Column + colDelta;

        // Case 1: Same row
        if (rowDelta == 0) {
            for (; column != toColumn; column += colDelta) {
                var piece = pieces[Row, column];
                if (piece != null) return true;
            }
            return false;
        }

        // Case 2: Same Column
        if (colDelta == 0) {
            for (; row != toRow; row += rowDelta) {
                var piece = pieces[row, Column];
                if (piece != null) return true;
            }
            return false;
        }


        // Case 3: Different row and column
        for (; row != toRow && column != toColumn; row += rowDelta, column += colDelta) {
            var piece = pieces[row, column];
            if (piece != null) return true;
        }

        return false;
    }


    public bool IsDiagonal(int toRow, int toColumn) {
        // Use Baby Chen (2020) Formula
        var a = Math.Abs(toRow - Row);
        var b = Math.Abs(toColumn - Column);
        return a == b;
    }

    public void UpdateState(Move move) {
        var (_, _, toRow, toColumn) = move.ToCoordinates();
        Logger.Log($"Updating {this}..");
        Row = toRow;
        Column = toColumn;
        HasMoved = true;
        Logger.Log($"Finished updating {this}");
    }

    public bool IsRankOrFile(int toRow, int toColumn) {
        return (Row == toRow || Column == toColumn);
    }

    public bool IsBlack() {
        return Colour == ChessPiece.EColour.Black;
    }

    public bool IsWhite() {
        return Colour == ChessPiece.EColour.Black;
    }


    public override string ToString() {
        return $"{Colour} {Name} at {Row},{Column}";
    }

    public ChessPiece Clone() {
        return (ChessPiece)this.MemberwiseClone();
    }

    public enum EColour {
        Black, White
    }

    public enum EName {
        None, // Annoyingly necessary because of Move.PieceToPromoteTo
        Rook,
        Knight,
        King,
        Queen,
        Pawn,
        Bishop
    }

}
