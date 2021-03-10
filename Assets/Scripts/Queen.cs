public class Queen : ChessPiece {
    public Queen(EColour colour, int row, int column) {
        Colour = colour;
        Name = EName.Queen;
        Row = row;
        Column = column;
    }

    public override bool CheckMove(ChessPiece[,] pieces, Move move) {
        var (_, _, toRow, toColumn) = move.ToCoordinates();
        if (!base.CheckMove(pieces, move)) return false;

        // FIDE 3.5 "[...] the queen [...] may not move over any intervening pieces."
        if (HasPiecesOnInterveningSquares(pieces, toRow, toColumn)) {
            // Debug.Log($"Unable to move Queen from {Row},{Column} to {toRow},{toColumn} - pieces in the way");
            return false;
        }

        // FIDE 3.4 The queen may move to any square along the file, the rank or a diagonal 
        // on which it stands.
        if (!IsDiagonal(toRow, toColumn) && !IsRankOrFile(toRow, toColumn)) {
            // Debug.Log($"Unable to move Queen from {Row},{Column} to {toRow},{toColumn} - must be on diagonal or rank or file");
            return false;
        }

        return true;
    }

    public override int GetScore() {
        return 20;
    }
}
