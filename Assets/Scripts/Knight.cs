using System;

public class Knight : ChessPiece {
    public Knight(EColour colour, int row, int column) {
        Colour = colour;
        Name = EName.Knight;
        Row = row;
        Column = column;
    }

    public override bool CheckMove(ChessPiece[,] pieces, Move move) {
        var (_, _, toRow, toColumn) = move.ToCoordinates();
        if (!base.CheckMove(pieces, move)) return false;

        // FIDE 3.6 The knight may move to one of the squares nearest to that on which 
        // it stands but not on the same rank, file or diagonal

        int rowDelta = Math.Abs(Row - toRow);
        int colDelta = Math.Abs(Column - toColumn);

        if (rowDelta == 0 || colDelta == 0) {
            // Logger.Log("Moves", $"Knight cannot move from {Row},{Column} to {toRow},{toColumn} - must not be on the same row or column");
            return false;
        }

        int sum = rowDelta + colDelta;

        return sum == 3;
    }

    public override int GetScore() {
        return 3;
    }
}