public class Rook : ChessPiece {
    public Rook(EColour colour, int row, int column) {
        Colour = colour;
        Name = EName.Rook;
        Row = row;
        Column = column;
    }

    public override bool CheckMove(ChessPiece[,] pieces, Move move) {
        var (_, _, toRow, toColumn) = move.ToCoordinates();
        if (!base.CheckMove(pieces, move)) return false;

        // FIDE 3.5 "[...] the rook [...] may not move over any intervening pieces."
        if (HasPiecesOnInterveningSquares(pieces, toRow, toColumn)) {
            // Debug.Log($"Unable to move Rook from {Row},{Column} to {toRow},{toColumn} - pieces in the way");
            return false;
        }

        // FIDE 3.3 The rook may move to any square along the file or the rank on which it stands
        if (!IsRankOrFile(toRow, toColumn)) {
            // Debug.Log($"Unable to move Rook from {Row},{Column} to {toRow},{toColumn} - not same rank or file");
            return false;
        }

        return true;
    }

    public override int GetScore() {
        return 5;
    }
}