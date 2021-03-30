using System;
using UnityEngine;
public class King : ChessPiece {
    public King(EColour colour, int row, int column) {
        Name = EName.King;
        Colour = colour;
        Row = row;
        Column = column;
    }

    public override bool CheckMove(ChessPiece[,] pieces, Move move) {
        var (_, _, toRow, toColumn) = move.ToCoordinates();
        // FIDE 3.8 a. 
        // There are two different ways of moving the king: by moving to any adjoining square
        // or by castling.
        if (IsCastling(pieces, move)) {
            move.IsCastling = true;
            return true;
        }

        if (!base.CheckMove(pieces, move)) return false;

        // Not a castle. Check to make sure it's the adjoining square.
        int rowDelta = Math.Abs(Row - toRow);
        int colDelta = Math.Abs(Column - toColumn);

        if (rowDelta > 1 || colDelta > 1) {
            // Logger.Log("Moves", $"Unable to move King from {Row},{Column} to {toRow},{toColumn} - {rowDelta},{colDelta}");
            return false;
        }

        return true;
    }

    public bool IsCastling(ChessPiece[,] pieces, Move move) {
        var (fromRow, _, toRow, toColumn) = move.ToCoordinates();
        // We can't do this check in strict order of the FIDE rules both for optimisation and
        // for sanity reasons, eg. we need to check there are no intervening pieces before we can
        // move the king along its squares in order to check if it will be attacked.

        // FIDE 3.8 a continued:
        // This is a move of the king and either rook of the same colour
        // on the player’s first rank
        // Logger.Log("IsCastling", "In IsCastling..");


        // Check for first rank
        var firstRank = IsBlack() ? 0 : 7;

        if (fromRow != firstRank || toRow != firstRank) {
            // Logger.Log("IsCastling", $"{toRow} is not {firstRank}");
            return false;
        }

        // Check for castling columns
        if (toColumn != 6 && toColumn != 2) {
            // Logger.Log("IsCastling", $"{toColumn} is not 6 or 2");
            return false;
        }

        var rookColumn = toColumn == 6 ? 7 : 0;

        // (b) if there is any piece between the king and the rook with which castling is
        // to be effected.
        if (HasPiecesOnInterveningSquares(pieces, toRow, rookColumn)) {
            // Logger.Log("IsCastling", $"There are pieces on the intervening squares");
            return false;
        }

        var rook = pieces[toRow, rookColumn];
        if (rook == null) {
            // Logger.Log("IsCastling", $"No piece at {toRow},{toColumn}");
            return false;
        }

        if (rook.Name != ChessPiece.EName.Rook || rook.Colour != Colour) {
            // Logger.Log("IsCastling", $"{rook} is not {Colour} Rook");
            return false;
        }

        // FIDE 3.8b(1)
        // "The right to castle has been lost.."
        // "(a) if the king has already moved"
        if (HasMoved) {
            // Logger.Log("IsCastling", $"{this} has moved");
            return false;
        }

        // "(b) with a rook that has already moved"
        if (rook.HasMoved) {
            // Logger.Log("IsCastling", $"{rook} has moved");
            return false;
        }

        // FIDE 3.8b(2)
        // "Castling is prevented temporarily:
        // (a) if the square on which the king stands..
        if (IsInCheck(pieces)) {
            return false;
        }

        // ..[if] the square which it must cross..
        var squareToCross = toColumn == 6 ? 5 : 3;
        var destination = toColumn;
        var originalColumn = Column;

        // Fake move the king and see if it'll put us in check.
        pieces[Row, Column] = null;
        var p = pieces[Row, squareToCross];

        if (p != null) {
            throw new Exception($"INVALID STATE: Found {p}. Attempted castle was {move}");
        }
        pieces[Row, squareToCross] = this;
        Column = squareToCross;

        // ..is attacked by one or more of the the opponent's pieces
        var isBeingAttackedOnCrossSquare = IsInCheck(pieces);

        // ..or the square which it is to occupy..
        pieces[Row, squareToCross] = null;
        p = pieces[Row, destination];

        if (p != null) {
            throw new Exception($"INVALID STATE: Found {p}");
        }
        pieces[Row, destination] = this;
        Column = destination;

        // ..is attacked by one or more of the the opponent's pieces"
        var isBeingAttackedOnDestination = IsInCheck(pieces);

        pieces[Row, originalColumn] = this;
        pieces[Row, destination] = null;
        p = pieces[Row, destination];

        if (p != null) {
            throw new Exception($"INVALID STATE: Found {p}");
        }
        Column = originalColumn;

        if (isBeingAttackedOnCrossSquare || isBeingAttackedOnDestination) {
            // Logger.Log("IsCastling", $"Square to be crossed {squareToCross} is being attacked");
            return false;
        }

        return true;
    }

    public override int GetScore() {
        return 100;
    }

    public bool IsInCheck(ChessPiece[,] pieces) {
        // Search through each piece in the board and see if they could legally put the King in check.
        foreach (var piece in pieces) {
            if (piece == null) continue;
            if (piece.Colour == Colour) continue;

            var move = new Move(piece.Row, piece.Column, Row, Column);
            if (piece.CheckMove(pieces, move)) {
                // Logger.Log("Moves", $"{piece.Colour} {piece.Name} at {piece.Row},{piece.Column} can put {Colour} King in check");
                return true;
            }

        }
        return false;
    }
}