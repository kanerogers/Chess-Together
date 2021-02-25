using System;
public class Pawn : ChessPiece {
    public Pawn(EColour colour, int row, int column) {
        Colour = colour;
        Name = EName.Pawn;
        Row = row;
        Column = column;
    }

    public override bool CheckMove(ChessPiece[,] pieces, int toRow, int toColumn) {
        // Check basic constraints first
        if (!base.CheckMove(pieces, toRow, toColumn)) {
            return false;
        }

        int forward = GetForward(toRow);

        // FIDE 3.7.a
        // Pawn cannot move backwards, or more than 2 forwards.
        if (forward < 1 || forward > 2) {
            // Debug.Log($"Pawn at {Row},{Column} cannot move to {toRow},{toColumn} as forward is {forward}");
            return false;
        }

        // FIDE 3.7.c
        // "the pawn may move to a square occupied by an opponent’s piece, which is
        // diagonally in front of it on an adjacent file, capturing that piece."
        var pieceOnSquare = pieces[toRow, toColumn];

        if (toColumn != Column) {
            int delta = Math.Abs(toColumn - Column);
            if (delta != 1) {
                // Debug.Log($"Pawn at {Row},{Column} cannot move to {toRow},{toColumn} as the column delta is {delta}");
                return false;
            }

            if (forward != 1) {
                // Debug.Log($"Pawn at {Row},{Column} cannot move to {toRow},{toColumn} as the column is not the same but forward is {forward}.");
                return false;
            }

            // Check if there is a piece on this diagonal
            if (pieceOnSquare != null) {
                Logger.Log("Pawn", $"Pawn can move diagonally to {toRow},{toColumn} as there is a piece on that square: {pieceOnSquare}");
                return true;
            } else {
                // Debug.Log($"Pawn cannot move to {toRow},{toColumn} as there is no piece on that square");
                return false;
            }
        }

        // We have proven that we are moving forward 1 or 2 squares.

        // Next, ensure there is no piece on that square
        if (pieceOnSquare != null) {
            // Debug.Log($"Pawn cannot move to {toRow},{toColumn} as there is a piece on that square");
            return false;
        }

        // FIDE 3.7.b
        // "on its first move the pawn [...] may advance two squares along the same file"
        if (forward == 2) {
            if (!OnStartingRow()) {
                // Debug.Log($"Pawn cannot move to {toRow},{toColumn} as it is not on the starting row.");
                return false;
            }

            // "..provided both squares are unoccupied"
            if (HasPiecesOnInterveningSquares(pieces, toRow, toColumn)) {
                // Debug.Log($"Pawn cannot move to {toRow},{toColumn} as there is a piece in the way.");
                return false;
            }
        }

        // TODO: FIDE 3.7e - Promotion

        return true;
    }

    bool OnStartingRow() {
        if (Colour == ChessPiece.EColour.Black) return Row == 1;
        return Row == 6;
    }

    int GetForward(int toRow) {
        if (IsBlack()) return toRow - Row;
        return Row - toRow;
    }

    public override int GetScore() {
        return 1;
    }

    public bool IsPromotion(int toRow, int toColumn) {
        var promotionRow = IsBlack() ? 7 : 0;
        var isPromotion = toRow == promotionRow;
        return isPromotion;
    }
}