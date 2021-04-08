﻿using System;
using System.Collections.Generic;
using System.Text;

public class ChessBoard {
    public King BlackKing;
    public King WhiteKing;
    public Dictionary<ChessPiece.EColour, List<Move>> ValidMoves = new Dictionary<ChessPiece.EColour, List<Move>>();
    public ChessPiece[,] Pieces = new ChessPiece[8, 8];
    public static ChessBoard Inst;
    public int Turn;
    public Stack<(Move, ChessPiece)> UndoStack = new Stack<(Move, ChessPiece)>();
    public Dictionary<ChessPiece.EColour, BoardStatus> State = new Dictionary<ChessPiece.EColour, BoardStatus>();
    public Pawn PawnThatCanBeCapturedWithEnpassant = null;
    static StringBuilder sb = new StringBuilder();
    public ChessPiece.EColour CanMove;

    #region Public

    public ChessBoard(bool emptyBoard = false) {
        Turn = 0;
        Inst = this;
        CanMove = ChessPiece.EColour.White;
        ValidMoves[ChessPiece.EColour.Black] = new List<Move>();
        ValidMoves[ChessPiece.EColour.White] = new List<Move>();

        State[ChessPiece.EColour.Black] = BoardStatus.NotInCheck;
        State[ChessPiece.EColour.White] = BoardStatus.NotInCheck;

        if (emptyBoard) return;

        SetUpBoard();
        UpdateBoardStatus();
    }

    public bool Move(int fromRow, int fromColumn, int toRow, int toColumn, bool checkIfKingIsInCheck = true, bool checkStateAfterMove = true) {
        var move = new Move(fromRow, fromColumn, toRow, toColumn);
        return Move(move, checkIfKingIsInCheck, checkStateAfterMove);
    }

    public bool Move(Move move, bool checkIfKingIsInCheck = true, bool checkStateAfterMove = true) {
        // Is this move valid?
        if (!IsValid(move, checkIfKingIsInCheck)) return false;

        // Move is valid! Update state.
        UpdateState(move);

        // Check the status of the board.
        if (checkStateAfterMove) UpdateBoardStatus();

        // Done!
        return true;
    }

    public bool IsValid(Move move, bool checkIfKingIsInCheck) {
        var (fromRow, fromColumn, toRow, toColumn) = move.ToCoordinates();
        var piece = Pieces[fromRow, fromColumn];

        // Does the piece exist?
        if (piece == null) {
            // Logger.Log("MOVES", $"Piece at {fromRow},{fromColumn} does not exist.");
            return false;
        }

        // Is it this piece's turn to move?
        if (piece.Colour != CanMove) {
            return false;
        }

        // Is this a valid move?
        if (!piece.CheckMove(Pieces, move)) {
            // Logger.Log("MOVES", $"Move {move} is invalid.");
            return false;
        }

        // Would this put us in check?
        if (checkIfKingIsInCheck && WouldPutKingInCheck(move)) return false;

        return true;
    }

    private void UpdateState(Move move) {
        var (fromRow, fromColumn, toRow, toColumn) = move.ToCoordinates();
        var piece = Pieces[fromRow, fromColumn];

        // Before we can update the state, we need to stash the move away.
        var removedPiece = Pieces[toRow, toColumn];

        // If this was an enpassant capture, the piece is at a different location.
        if (move.IsEnPassantCapture) {
            var enPassantRow = ((Pawn)piece).GetEnPassantRow(toRow);
            if (enPassantRow > 8 || enPassantRow < 0) {
                throw new ChessException($"Invalid enPassantRow {enPassantRow}. Piece is {piece} ???");
            }
            removedPiece = Pieces[enPassantRow, toColumn];
            // Logger.Log("EP", $"Removed piece is {removedPiece}");
        }

        UndoStack.Push((move, removedPiece));

        // Update state.
        UpdatePiecesState(move);
        UpdateTurn(move);
        UpdateEnPassantState(move);

        if (PawnThatCanBeCapturedWithEnpassant?.Row == 1) {
            throw new ChessException($"Impossible {PawnThatCanBeCapturedWithEnpassant} - invalidated pawn cannot be on row 1 and CanBeCapturedByEnpassant");
        }


        // Logger.Log($"..done!");
    }

    public void Undo(bool shouldUpdateBoardStatus = true) {
        var (lastMove, removedPiece) = UndoStack.Pop();
        var (invalidatedPawn, updatedPawn) = lastMove.PreviousEnPassantState;

        if (invalidatedPawn?.Row == 1) {
            // throw new ChessException($"Impossible {invalidatedPawn} - invalidated pawn cannot be on row 1 and CanBeCapturedByEnpassant");
        }

        if (updatedPawn?.Row == 1) {
            // throw new ChessException($"Impossible {updatedPawn} - invalidated pawn cannot be on row 1 and CanBeCapturedByEnpassant");
        }

        UndoPiecesState(lastMove, removedPiece);
        UndoTurn(lastMove);
        UndoEnPassantState(lastMove);

        if (shouldUpdateBoardStatus) UpdateBoardStatus();

    }

    public bool IsValidMove(ChessPiece piece, int toRow, int toColumn) {
        var validMoves = ValidMoves[piece.Colour];
        var move = new Move(piece.Row, piece.Column, toRow, toColumn);

        return validMoves.Contains(move);
    }

    public Move GetLastMove() {
        var (move, _) = UndoStack.Peek();
        return move;
    }

    public ChessPiece CreatePiece(ChessPiece.EName name, int row, int column, ChessPiece.EColour colour) {
        ChessPiece piece = null;
        if (name == ChessPiece.EName.King) {
            piece = new King(colour, row, column);
            if (colour == ChessPiece.EColour.Black) {
                BlackKing = (King)piece;
            } else {
                WhiteKing = (King)piece;
            }
        }

        if (name == ChessPiece.EName.Bishop) {
            piece = new Bishop(colour, row, column);
        }

        if (name == ChessPiece.EName.Knight) {
            piece = new Knight(colour, row, column);
        }

        if (name == ChessPiece.EName.Pawn) {
            piece = new Pawn(colour, row, column);
        }

        if (name == ChessPiece.EName.Queen) {
            piece = new Queen(colour, row, column);
        }

        if (name == ChessPiece.EName.Rook) {
            piece = new Rook(colour, row, column);
        }

        Pieces[row, column] = piece;
        return piece;
    }

    // Checks through the enemy's pieces and does a couple things:
    // - Lists all their valid moves
    // - Determines whether they're in check/checkmate/etc.
    public void UpdateBoardStatus() {
        // Take a deep breath..

        // Reset the states.
        State[ChessPiece.EColour.Black] = BoardStatus.NotInCheck;
        State[ChessPiece.EColour.White] = BoardStatus.NotInCheck;

        var king = CanMove.IsBlack() ? BlackKing : WhiteKing;

        // If a King is in Check, assume it's Checkmate UNLESS we can prove otherwise.
        if (king.IsInCheck(Pieces)) {
            State[CanMove] = BoardStatus.Checkmate;
        }

        // Clear all our valid moves.
        ValidMoves[ChessPiece.EColour.Black].Clear();
        ValidMoves[ChessPiece.EColour.White].Clear();
        var hasEp = PawnThatCanBeCapturedWithEnpassant != null;

        // Iterate through all pieces on the board and check their valid moves.
        for (int fromRow = 0; fromRow < 8; fromRow++) {
            for (int fromColumn = 0; fromColumn < 8; fromColumn++) {
                var piece = Pieces[fromRow, fromColumn];
                if (piece == null) continue;
                var colour = piece.Colour;

                if (piece.Row != fromRow) throw new ChessException($"INVALID STATE: {piece} should have its row set to {fromRow}!!");
                if (piece.Column != fromColumn) throw new ChessException($"INVALID STATE: {piece} should have its column set to {fromColumn}!!");

                if (colour != CanMove) continue;

                for (int toRow = 0; toRow < 8; toRow++) {
                    for (int toColumn = 0; toColumn < 8; toColumn++) {
                        var move = new Move(fromRow, fromColumn, toRow, toColumn);
                        var pieceAtDestination = Pieces[toRow, toColumn];

                        // If this move is invalid, continue.
                        if (!Move(move, checkIfKingIsInCheck: true, checkStateAfterMove: false)) continue;

                        // WARNING: Move has been made, must call Undo before continue!

                        // Check to see if this move is a pawn promotion.
                        bool isPromotion = false;
                        if (piece.Name == ChessPiece.EName.Pawn) {
                            var pawn = (Pawn)piece;
                            isPromotion = pawn.IsPromotion(toRow, toColumn);
                        }

                        // If it is a promotion, we have to add all the promotion possibilities.
                        // TODO: Extract to method on Pawn?
                        if (isPromotion) {
                            // FIDE 3.7e - ..
                            // exchanged as part of the same move on the same square for a new queen, rook,
                            // bishop or knight of the same colour.
                            move = new Move(fromRow, fromColumn, toRow, toColumn, ChessPiece.EName.Bishop);
                            ValidMoves[colour].Add(move);
                            move = new Move(fromRow, fromColumn, toRow, toColumn, ChessPiece.EName.Rook);
                            ValidMoves[colour].Add(move);
                            move = new Move(fromRow, fromColumn, toRow, toColumn, ChessPiece.EName.Queen);
                            ValidMoves[colour].Add(move);
                            move = new Move(fromRow, fromColumn, toRow, toColumn, ChessPiece.EName.Knight);
                            ValidMoves[colour].Add(move);
                        } else {
                            ValidMoves[colour].Add(move);
                        }

                        // If the King is not in check, there is no need to evaluate if this is checkmate.
                        if (State[colour] == BoardStatus.NotInCheck) {
                            Undo(false);
                            continue;
                        }

                        // If this move would put the King out of check, then we know we're not in checkmate.
                        // not in Checkmate.
                        if (!king.IsInCheck(Pieces)) {
                            State[colour] = BoardStatus.Check;
                        }

                        Undo(false);
                    }
                }
            }
        }

        // If we're not in checkmate and there are no valid moves left, this is a stalemate.
        if (State[CanMove] == BoardStatus.Checkmate) return;
        if (ValidMoves[CanMove].Count == 0) State[CanMove] = BoardStatus.Stalemate;
    }
    #endregion

    #region Private
    private void UpdatePiecesState(Move move) {
        if (move.PieceToPromoteTo != ChessPiece.EName.None) {
            PromotePiece(move);
            return;
        }

        var (fromRow, fromColumn, toRow, toColumn) = move.ToCoordinates();
        var piece = Pieces[fromRow, fromColumn];

        // If this was the first time the piece moved, set that flag on Move
        if (!piece.HasMoved) move.FirstMoved = true;

        // Update the piece's bookkeeping
        piece.UpdateState(move);

        // Update our own bookkeeping.
        Pieces[toRow, toColumn] = piece;
        Pieces[fromRow, fromColumn] = null;

        if (move.IsEnPassantCapture) {
            var enPassantRow = ((Pawn)piece).GetEnPassantRow(toRow);
            // Logger.Log($"Setting {enPassantRow},{toColumn} to null");
            Pieces[enPassantRow, toColumn] = null;
        }

        if (move.IsCastling) {
            DoCastle(move);
        }
    }

    private void UndoPiecesState(Move lastMove, ChessPiece removedPiece) {
        // Promotion is a special case, handle that on its own.
        if (lastMove.IsPromotion()) {
            UndoPromotion(lastMove, removedPiece);
            return;
        }

        var (fromRow, fromColumn, toRow, toColumn) = lastMove.ToCoordinates();
        var movedPiece = Pieces[toRow, toColumn];
        if (movedPiece == null) {
            throw new ChessException($"Unable to undo move {lastMove} - movedPiece is null!");
        }

        // If this was a castle, undo that too.
        if (lastMove.IsCastling) {
            UndoCastle(toRow, toColumn);
        }

        // Undo the change to the piece
        movedPiece.Undo(lastMove);

        // Put the piece back where it was
        Pieces[fromRow, fromColumn] = movedPiece;

        // If this was an en passant, the piece is in a different place.
        if (lastMove.IsEnPassantCapture) {
            // First, remove the pawn that moved there.
            Pieces[toRow, toColumn] = null;

            // Next, find where it should go.
            toRow = removedPiece.Row;
            // Logger.Log("EP", $"Restoring {removedPiece} to {toRow},{toColumn}");
        }

        // If a piece was removed, put it back.
        Pieces[toRow, toColumn] = removedPiece;
    }


    private void UpdateTurn(Move move) {
        Turn++;
        CanMove = CanMove.Inverse();
        // Logger.Log($"Turn updated - {Turn}");
    }

    private void UndoTurn(Move lastMove) {
        Turn--;
        CanMove = CanMove.Inverse();
    }

    private void UpdateEnPassantState(Move move) {
        var (_, _, toRow, toColumn) = move.ToCoordinates(); // note: use toRow, toColumn as pawn has already moved
        var piece = Pieces[toRow, toColumn];
        Pawn invalidatedPawn = null;
        Pawn updatedPawn = null;

        // If there was a pawn that previously could be captured, now it can't.
        invalidatedPawn = null;
        var epPawn = PawnThatCanBeCapturedWithEnpassant;


        // IMPORTANT! If our last move was the one that set our EnPassantState to begin with (eg. a double square pawn move)
        // we MUST check to make sure that we're not invalidating the move that was just made
        // TODO: shouldn't we do this BEFORE we update the rest of the state?
        if (epPawn != null && piece != epPawn) {
            invalidatedPawn = epPawn;
            // Logger.Log("UPDATE EP", $"{invalidatedPawn} now cannot be captured");
            invalidatedPawn.CanBeCapturedByEnpassant = false;
            PawnThatCanBeCapturedWithEnpassant = null;
            // Logger.Log("UPDATE EP", $"PawnThatCanBeCapturedWithEnpassant is now null");
        }

        if (piece.Name == ChessPiece.EName.Pawn) {
            var pawn = (Pawn)piece;
            if (pawn.CanBeCapturedByEnpassant) {
                if (pawn.Row == 1) {
                    throw new ChessException("holaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                }
                updatedPawn = pawn;
                PawnThatCanBeCapturedWithEnpassant = pawn;
                // Logger.Log("UPDATE EP", $"PawnThatCanBeCapturedWithEnpassant is now {pawn}");
            }
        }

        if (invalidatedPawn?.Row == 1) {
            // throw new ChessException($"Impossible {invalidatedPawn} - invalidated pawn cannot be on row 1 and CanBeCapturedByEnpassant");
        }

        move.PreviousEnPassantState = (invalidatedPawn, updatedPawn);
    }

    private void UndoEnPassantState(Move lastMove) {
        var (_, _, toRow, toColumn) = lastMove.ToCoordinates();
        var (invalidatedPawn, updatedPawn) = lastMove.PreviousEnPassantState;
        // Restore any en passant state

        // If null, then set to null again.
        PawnThatCanBeCapturedWithEnpassant = invalidatedPawn;

        // invalidatedPawn was set to false on the previous move, now set it to true.
        if (invalidatedPawn != null) {
            // Logger.Log("UNDO EP", $"Restoring EnPassant state for {invalidatedPawn} = true");
            invalidatedPawn.CanBeCapturedByEnpassant = true;
            // Logger.Log("UNDO EP", $"PawnThatCanBeCapturedWithEnpassant is now {invalidatedPawn}");
            if (PawnThatCanBeCapturedWithEnpassant?.Row == 1) {
                // throw new ChessException($"Impossible {invalidatedPawn} - invalidated pawn cannot be on row 1 and CanBeCapturedByEnpassant");
            }
        }


        // updatedPawn was set to true on the previous move, now set it to false.
        if (updatedPawn != null) {
            // Logger.Log("UNDO EP", $"Restoring EnPassant state for {toRow},{toColumn} = false");
            updatedPawn.CanBeCapturedByEnpassant = false;
        }
    }
    private void DoCastle(Move move) {
        var (fromRow, _, toRow, kingToColumn) = move.ToCoordinates();
        var rookOriginalColumn = kingToColumn == 6 ? 7 : 0;
        var rookNewColumn = rookOriginalColumn == 7 ? 5 : 3;
        var rook = Pieces[toRow, rookOriginalColumn];

        Pieces[fromRow, rookOriginalColumn] = null;
        Pieces[toRow, rookNewColumn] = rook;
        rook.Column = rookNewColumn;
    }

    private void UndoCastle(int row, int kingMovedToColumn) {
        // Where was the Rook originally?
        var originalColumn = kingMovedToColumn == 6 ? 7 : 0;

        // Where did the Rook move to?
        var newColumn = originalColumn == 7 ? 5 : 3;

        // Get the rook
        var rook = Pieces[row, newColumn];

        // Reset its position
        rook.Column = originalColumn;
        Pieces[row, newColumn] = null;
        Pieces[row, originalColumn] = rook;

        // Reset its HasMoved flag
        rook.HasMoved = false;
    }

    private bool WouldPutKingInCheck(Move move) {
        // Logger.Log("BOARD STATE", $"Evlauting if {move} would put King in check.");
        var (fromRow, fromColumn, _, _) = move.ToCoordinates();
        var piece = Pieces[fromRow, fromColumn];
        if (piece == null) throw new ChessException($"Error evaluating {move} - piece is null!");
        var king = piece.Colour == ChessPiece.EColour.Black ? BlackKing : WhiteKing;

        if (!Move(move, false, false)) {
            throw new System.Exception($"Attempted to make invalid move: {move}");
        }

        var isInCheck = (king.IsInCheck(Pieces));
        Undo(false);

        return isInCheck;
    }

    private void PromotePiece(Move move) {
        var (fromRow, fromColumn, toRow, toColumn) = move.ToCoordinates();
        var pawn = Pieces[fromRow, fromColumn];
        if (pawn == null) throw new ChessException($"Invalid promotion - no pawn found at {fromRow},{fromColumn}. Board state: {this}");

        // Kill the pawn.
        Pieces[fromRow, fromColumn] = null;

        // Create the piece.
        CreatePiece(move.PieceToPromoteTo, toRow, toColumn, pawn.Colour);
    }

    void SetUpBoard() {
        // Create Pieces 
        // // 0,0 - Black Queenside Rook
        CreatePiece(ChessPiece.EName.Rook, 0, 0, ChessPiece.EColour.Black);

        // // 0,1 - Black Queenside Knight
        CreatePiece(ChessPiece.EName.Knight, 0, 1, ChessPiece.EColour.Black);

        // // 0,2 - Black Queenside Bishop
        CreatePiece(ChessPiece.EName.Bishop, 0, 2, ChessPiece.EColour.Black);

        // // 0,3 - Black Queen
        CreatePiece(ChessPiece.EName.Queen, 0, 3, ChessPiece.EColour.Black);

        // // 0,4 - Black King
        CreatePiece(ChessPiece.EName.King, 0, 4, ChessPiece.EColour.Black);

        // // 0,5 - Black Kingside Bishop
        CreatePiece(ChessPiece.EName.Bishop, 0, 5, ChessPiece.EColour.Black);

        // // 0,6 - Black Kingside Knight
        CreatePiece(ChessPiece.EName.Knight, 0, 6, ChessPiece.EColour.Black);

        // // 0,7 - Black Kingside Rook
        CreatePiece(ChessPiece.EName.Rook, 0, 7, ChessPiece.EColour.Black);

        // // 1,0 - Black Pawn
        CreatePiece(ChessPiece.EName.Pawn, 1, 0, ChessPiece.EColour.Black);

        // // 1,1 - Black Pawn
        CreatePiece(ChessPiece.EName.Pawn, 1, 1, ChessPiece.EColour.Black);

        // // 1,2 - Black Pawn
        CreatePiece(ChessPiece.EName.Pawn, 1, 2, ChessPiece.EColour.Black);

        // // 1,3 - Black Pawn
        CreatePiece(ChessPiece.EName.Pawn, 1, 3, ChessPiece.EColour.Black);

        // // 1,4 - Black Pawn
        CreatePiece(ChessPiece.EName.Pawn, 1, 4, ChessPiece.EColour.Black);

        // // 1,5 - Black Pawn
        CreatePiece(ChessPiece.EName.Pawn, 1, 5, ChessPiece.EColour.Black);

        // // 1,6 - Black Pawn
        CreatePiece(ChessPiece.EName.Pawn, 1, 6, ChessPiece.EColour.Black);

        // // 1,7 - Black Pawn
        CreatePiece(ChessPiece.EName.Pawn, 1, 7, ChessPiece.EColour.Black);

        // // 6,0 - White Pawn
        CreatePiece(ChessPiece.EName.Pawn, 6, 0, ChessPiece.EColour.White);

        // // 6,1 - White Pawn
        CreatePiece(ChessPiece.EName.Pawn, 6, 1, ChessPiece.EColour.White);

        // // 6,2 - White Pawn
        CreatePiece(ChessPiece.EName.Pawn, 6, 2, ChessPiece.EColour.White);

        // // 6,3 - White Pawn
        CreatePiece(ChessPiece.EName.Pawn, 6, 3, ChessPiece.EColour.White);

        // // 6,4 - White Pawn
        CreatePiece(ChessPiece.EName.Pawn, 6, 4, ChessPiece.EColour.White);

        // // 6,5 - White Pawn
        CreatePiece(ChessPiece.EName.Pawn, 6, 5, ChessPiece.EColour.White);

        // // 6,6 - White Pawn
        CreatePiece(ChessPiece.EName.Pawn, 6, 6, ChessPiece.EColour.White);

        // // 6,7 - White Pawn
        CreatePiece(ChessPiece.EName.Pawn, 6, 7, ChessPiece.EColour.White);

        // // 7,0 - White Queenside Rook
        CreatePiece(ChessPiece.EName.Rook, 7, 0, ChessPiece.EColour.White);

        // // 7,1 - White Queenside Knight
        CreatePiece(ChessPiece.EName.Knight, 7, 1, ChessPiece.EColour.White);

        // // 7,2 - White Queenside Bishop
        CreatePiece(ChessPiece.EName.Bishop, 7, 2, ChessPiece.EColour.White);

        // // 7,3 - White Queen
        CreatePiece(ChessPiece.EName.Queen, 7, 3, ChessPiece.EColour.White);

        // // 7,4 - White King
        CreatePiece(ChessPiece.EName.King, 7, 4, ChessPiece.EColour.White);

        // // 7,5 - White Kingside Bishop
        CreatePiece(ChessPiece.EName.Bishop, 7, 5, ChessPiece.EColour.White);

        // // 7,6 - White Kingside Knight
        CreatePiece(ChessPiece.EName.Knight, 7, 6, ChessPiece.EColour.White);

        // // 7,7 - White Kingside Rook
        CreatePiece(ChessPiece.EName.Rook, 7, 7, ChessPiece.EColour.White);

    }

    private void UndoPromotion(Move lastMove, ChessPiece removedPiece) {
        var (fromRow, fromColumn, toRow, toColumn) = (lastMove.FromRow, lastMove.FromColumn, lastMove.ToRow, lastMove.ToColumn);

        // Grab the promotion piece
        var promotionPiece = Pieces[toRow, toColumn];

        // Create a new pawn to replace the one that was promoted.
        var pawn = new Pawn(promotionPiece.Colour, fromRow, fromColumn);

        // Replace the promotion piece.
        Pieces[toRow, toColumn] = removedPiece;

        // Put the pawn back in place
        Pieces[fromRow, fromColumn] = pawn;
    }

    #endregion // private

    #region Overrides
    public override string ToString() {
        sb.Clear();
        for (var row = 0; row <= 7; row++) {
            for (var column = 0; column <= 7; column++) {
                var p = Pieces[row, column];
                if (p == null) continue;
                sb.Append(row);
                sb.Append(", ");
                sb.Append(column);
                sb.Append(p.ToString());
                sb.Append(", ");
            }
        }
        return sb.ToString();
    }
    #endregion

    public enum BoardStatus {
        NotInCheck,
        Check,
        Checkmate,
        Stalemate
    }
}