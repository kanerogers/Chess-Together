public class Bishop : ChessPiece {
    public Bishop(EColour colour, int row, int column) {
        Colour = colour;
        Name = EName.Bishop;
        Row = row;
        Column = column;
    }

    public override bool CheckMove(ChessPiece[,] pieces, Move move) {
        var (_, _, toRow, toColumn) = move.ToCoordinates();
        if (!base.CheckMove(pieces, move)) {
            // Logger.Log("Moves", $"Unable to move Bishop from {Row},{Column} to {toRow},{toColumn} - CheckMove failed");
            return false;
        }

        // FIDE 3.5 "[...] the bishop [...] may not move over any intervening pieces."
        if (HasPiecesOnInterveningSquares(pieces, toRow, toColumn)) {
            // Logger.Log("Moves", $"Unable to move Bishop from {Row},{Column} to {toRow},{toColumn} - pieces in the way");
            return false;
        }

        // FIDE 3.2 "The bishop may move to any square along a diagonal on which it stands."
        if (!IsDiagonal(toRow, toColumn)) {
            // Logger.Log("Moves", $"Unable to move Bishop from {Row},{Column} to {toRow},{toColumn} - non diagonal move");
            return false;
        }

        return true;
    }

    public override int GetScore() {
        return 3;
    }
}