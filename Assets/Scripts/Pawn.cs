using System;
public class Pawn : ChessPiece {
    public bool CanBeCapturedByEnpassant = false;

    public Pawn(EColour colour, int row, int column) {
        Colour = colour;
        Name = EName.Pawn;
        Row = row;
        Column = column;
    }

    public override bool CheckMove(ChessPiece[,] pieces, Move move) {
        var (_, _, toRow, toColumn) = move.ToCoordinates();
        // Check basic constraints first
        if (!base.CheckMove(pieces, move)) {
            return false;
        }

        int forward = GetForward(toRow);

        // FIDE 3.7.a
        // Pawn cannot move backwards, or more than 2 forwards.
        if (forward < 1 || forward > 2) {
            Logger.Log($"Pawn at {Row},{Column} cannot move to {toRow},{toColumn} as forward is {forward}");
            return false;
        }

        // FIDE 3.7.c
        // "the pawn may move to a square occupied by an opponent’s piece, which is
        // diagonally in front of it on an adjacent file, capturing that piece."
        var pieceOnSquare = pieces[toRow, toColumn];

        if (toColumn != Column) {
            int delta = Math.Abs(toColumn - Column);
            if (delta != 1) {
                Logger.Log($"Pawn at {Row},{Column} cannot move to {toRow},{toColumn} as the column delta is {delta}");
                return false;
            }

            if (forward != 1) {
                Logger.Log($"Pawn at {Row},{Column} cannot move to {toRow},{toColumn} as the column is not the same but forward is {forward}.");
                return false;
            }

            // Check if there is a piece on this diagonal
            if (pieceOnSquare != null) {
                Logger.Log("Pawn", $"Pawn can move diagonally to {toRow},{toColumn} as there is a piece on that square: {pieceOnSquare}");
                return true;
            } else {
                // Check if this is en passant
                if (IsEnPassant(toRow, toColumn, pieces)) return true;

                Logger.Log($"Pawn cannot move to {toRow},{toColumn} as there is no piece on that square");
                return false;
            }
        }

        // We have proven that we are moving forward 1 or 2 squares.

        // Next, ensure there is no piece on that square
        if (pieceOnSquare != null) {
            Logger.Log($"Pawn cannot move to {toRow},{toColumn} as there is a piece on that square");
            return false;
        }

        // FIDE 3.7.b
        // "on its first move the pawn [...] may advance two squares along the same file"
        if (forward == 2) {
            if (!OnStartingRow()) {
                Logger.Log($"Pawn cannot move to {toRow},{toColumn} as it is not on the starting row.");
                return false;
            }

            // "..provided both squares are unoccupied"
            if (HasPiecesOnInterveningSquares(pieces, toRow, toColumn)) {
                Logger.Log($"Pawn cannot move to {toRow},{toColumn} as there is a piece in the way.");
                return false;
            }

            // Set the CanBeCapturedByEnPassant flag
            Logger.Log("PAWN", $"Setting enPassant flag for {toRow},{toColumn} to true");
            CanBeCapturedByEnpassant = true;
        }

        return true;
    }

    // FIDE 3.7d
    // A pawn attacking a square crossed by an opponent’s pawn which has advanced two
    // squares in one move from its original square may capture this opponent’s pawn as
    // though the latter had been moved only one square. 
    private bool IsEnPassant(int toRow, int toColumn, ChessPiece[,] pieces) {
        // First, find the EnPassant square.
        var enPassantRow = GetEnPassantRow(toRow);
        var piece = pieces[enPassantRow, toColumn];
        if (piece == null) return false;

        if (piece.Name != ChessPiece.EName.Pawn || piece.Colour == Colour) return false;

        var pawn = (Pawn)piece;
        // "..This capture is only legal on the move following this advance"
        if (!pawn.CanBeCapturedByEnpassant) {
            return false;
        }

        return true;
    }

    int GetEnPassantRow(int toRow) {
        var backwardsOneSquare = IsBlack() ? 1 : -1;
        var enPassantRow = toRow - backwardsOneSquare;
        return enPassantRow;
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