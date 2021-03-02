using System;
using System.Collections.Generic;

public class ChessBoard {
    public King BlackKing;
    public King WhiteKing;
    public Dictionary<ChessPiece.EColour, List<Move>> ValidMoves = new Dictionary<ChessPiece.EColour, List<Move>>();
    public ChessPiece[,] Pieces = new ChessPiece[8, 8];
    public static ChessBoard Inst;
    public int Turn;
    public Stack<(Move, ChessPiece)> UndoStack = new Stack<(Move, ChessPiece)>();
    public Dictionary<ChessPiece.EColour, BoardState> State = new Dictionary<ChessPiece.EColour, BoardState>();
    public Dictionary<(int, int), Boolean> EnPassantState = new Dictionary<(int, int), bool>();

    #region Public
    public ChessBoard() {
        Inst = this;
        SetUpBoard();
        ValidMoves[ChessPiece.EColour.Black] = new List<Move>();
        ValidMoves[ChessPiece.EColour.White] = new List<Move>();
        CheckBoard();
    }

    public ChessBoard(bool emptyBoard) {
        Inst = this;
        ValidMoves[ChessPiece.EColour.Black] = new List<Move>();
        ValidMoves[ChessPiece.EColour.White] = new List<Move>();

        if (emptyBoard) return;
        SetUpBoard();

        State[ChessPiece.EColour.Black] = BoardState.NotInCheck;
        State[ChessPiece.EColour.White] = BoardState.NotInCheck;
        CheckBoard();
    }

    public bool Move(Move move, bool lazy = false) {
        if (move.PieceToPromoteTo != ChessPiece.EName.None) {
            return PromotePiece(move, lazy);
        }

        return Move(move.FromRow, move.FromColumn, move.ToRow, move.ToColumn, lazy);
    }


    public bool Move(int fromRow, int fromColumn, int toRow, int toColumn, bool lazy = false) {
        var piece = Pieces[fromRow, fromColumn];
        if (piece == null) {
            // Logger.Log("Moves", $"Error - No piece found at {fromRow},{fromColumn}");
            return false;
        }

        // First, check if this is a castle. Because castling is fucking whack.
        bool isCastling = false;

        // Store the original column for this move as castling will mangle it.
        int originalColumn = toColumn;
        bool isKing = piece.Name == ChessPiece.EName.King;

        if (isKing) {
            var king = (King)piece;
            isCastling = king.IsCastling(Pieces, toRow, toColumn);
        }

        // No need to check move if we know this is a valid castle.
        if (isCastling) {
            // Change the destination column for castling.
            toColumn = GetCastlingDestination(toColumn);
        } else {
            // Is this move legal?
            if (!piece.CheckMove(Pieces, toRow, toColumn)) {
                // Logger.Log($"Moves", $"Cannot move {piece} to {toRow},{toColumn} - illegal move");
                return false;
            }

            if (!lazy) {
                if (WouldPutKingInCheck(piece, toRow, toColumn)) {
                    // Logger.Log("Moves", $"Cannot move {piece.Colour} {piece.Name} at {fromRow},{fromColumn} to {toRow},{toColumn}. Would put {piece.Colour} King in check.");

                    return false;
                }
            }
        }

        // Would this move put us into check?

        // OKAY! This move is valid, let's execute it.
        // Need to keep a record of whether King or Rook has moved.
        bool firstMoved = false;
        if (piece.Name == ChessPiece.EName.Rook || isKing) {
            if (!piece.HasMoved) {
                firstMoved = true;
                piece.HasMoved = true;
            }
        }


        // Need to move the rook as well..
        if (isCastling) {
            MoveRookForCastling(toRow, originalColumn);
        }

        // Stash the outcome of this move.
        var lastMove = new Move(fromRow, fromColumn, toRow, toColumn);
        lastMove.isCastling = isCastling;
        lastMove.firstMoved = firstMoved;
        var removedPiece = Pieces[toRow, toColumn];
        UndoStack.Push((lastMove, removedPiece));

        // Update en-passant state
        // Invalidate previous state
        foreach (var s in EnPassantState) {
            var (row, column) = s.Key;
            var pawn = Pieces[row, column];
            if (pawn != null) {
                var p = (Pawn)pawn;
                Logger.Log($"Setting {pawn} canbecaptured to false");
                p.CanBeCapturedByEnpassant = false;
            }
        }

        EnPassantState.Clear();

        // Set new state
        if (piece.Name == ChessPiece.EName.Pawn) {
            var pawn = (Pawn)piece;
            if (pawn.CanBeCapturedByEnpassant) {
                Logger.Log($"Setting state - Pawn at {toRow},{toColumn} canbecaptured to true");
                EnPassantState[(toRow, toColumn)] = true;
            }
        }

        // Update the piece's bookkeeping.
        piece.Column = toColumn;
        piece.Row = toRow;

        // Update our own bookkeeping.
        Pieces[toRow, toColumn] = piece;
        Pieces[fromRow, fromColumn] = null;


        // Update the state etc.
        if (!lazy) {
            CheckBoard();
        }

        Turn++;

        // Happy days.
        return true;
    }

    public bool IsValidMove(ChessPiece piece, int toRow, int toColumn) {
        var validMoves = ValidMoves[piece.Colour];
        var move = new Move(piece.Row, piece.Column, toRow, toColumn);

        return validMoves.Contains(move);
    }

    public void Undo() {
        var (lastMove, removedPiece) = UndoStack.Pop();
        var (toRow, toColumn) = (lastMove.ToRow, lastMove.ToColumn);

        if (lastMove.PieceToPromoteTo != ChessPiece.EName.None) {
            UndoPromotion(lastMove, removedPiece);
            return;
        }

        // First, move that piece back.
        var movedPiece = Pieces[toRow, toColumn];

        // If this was a castle, we need to do a bit of fuckery to undo this.
        if (lastMove.isCastling) {
            lastMove.FromColumn = 4; // King is always on 4.

            UndoCastle(toRow, toColumn);
        }

        // If this was a pawn, we need to re-set its 

        // If this move was the first time the piece moved, then reset its HasMoved flag
        if (lastMove.firstMoved) {
            movedPiece.HasMoved = false;
        }

        // If this was a promotion, this piece is actually a pawn.
        if (lastMove.PieceToPromoteTo != ChessPiece.EName.None) {
        }

        movedPiece.Row = lastMove.FromRow;
        movedPiece.Column = lastMove.FromColumn;

        Pieces[lastMove.FromRow, lastMove.FromColumn] = movedPiece;

        // Then put back the piece that was there (if null then keep as null)
        Pieces[lastMove.ToRow, lastMove.ToColumn] = removedPiece;
        Turn--;
    }


    public Move GetLastMove() {
        var (move, _) = UndoStack.Peek();
        return move;
    }

    public void CreatePiece(ChessPiece.EName name, int row, int column, ChessPiece.EColour colour) {
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
    }

    // Possibly overloaded function that: 
    // - Returns whether the player of a given colour is in checkmate
    // - Evaluates all valid moves for the player
    public void CheckBoard() {
        // Take a deep breath..
        State[ChessPiece.EColour.Black] = BoardState.NotInCheck;
        State[ChessPiece.EColour.White] = BoardState.NotInCheck;

        // If a King is in Check, assume it's Checkmate UNLESS we can prove otherwise.

        if (BlackKing.IsInCheck(Pieces)) {
            State[ChessPiece.EColour.Black] = BoardState.Checkmate;
        }

        if (WhiteKing.IsInCheck(Pieces)) {
            State[ChessPiece.EColour.White] = BoardState.Checkmate;
        }

        // Clear all our valid moves.
        ValidMoves[ChessPiece.EColour.Black].Clear();
        ValidMoves[ChessPiece.EColour.White].Clear();

        // Iterate through all pieces on the board and check their valid moves.
        foreach (var piece in Pieces) {
            if (piece == null) continue;
            var colour = piece.Colour;

            for (int toRow = 0; toRow < 8; toRow++) {
                for (int toColumn = 0; toColumn < 8; toColumn++) {
                    if (!piece.CheckMove(Pieces, toRow, toColumn)) continue;
                    if (WouldPutKingInCheck(piece, toRow, toColumn)) continue;

                    // Okay, it's legal. First, evaluate this move.

                    // If there is a piece of the opposite colour there, record its value.
                    int score = 0;

                    var enemyPiece = Pieces[toRow, toColumn];

                    if (enemyPiece != null) {
                        // Sanity check..
                        if (enemyPiece.Colour == piece.Colour) {
                            // Make sure this isn't just a castle though..
                            if (enemyPiece.Name != ChessPiece.EName.Rook) {
                                throw new System.Exception($"Invalid state: {piece} and {enemyPiece} are the same colour!");
                            }
                        } else {
                            score += enemyPiece.GetScore();
                        }
                    }

                    bool isPromotion = false;
                    if (piece.Name == ChessPiece.EName.Pawn) {
                        var pawn = (Pawn)piece;
                        isPromotion = pawn.IsPromotion(toRow, toColumn);
                    }

                    // Add this to our list of valid moves.
                    Move move = null;

                    // If it's a promotion, we have to add all the promotion possibilities and NOT let the pawn move to the end row.
                    if (isPromotion) {
                        // FIDE 3.7e - ..
                        // exchanged as part of the same move on the same square for a new queen, rook,
                        // bishop or knight of the same colour.
                        move = new Move(piece.Row, piece.Column, toRow, toColumn, ChessPiece.EName.Bishop);
                        ValidMoves[colour].Add(move);
                        move = new Move(piece.Row, piece.Column, toRow, toColumn, ChessPiece.EName.Rook);
                        ValidMoves[colour].Add(move);
                        move = new Move(piece.Row, piece.Column, toRow, toColumn, ChessPiece.EName.Queen);
                        ValidMoves[colour].Add(move);
                        move = new Move(piece.Row, piece.Column, toRow, toColumn, ChessPiece.EName.Knight);
                        ValidMoves[colour].Add(move);
                    } else {
                        move = new Move(piece.Row, piece.Column, toRow, toColumn, score);
                        ValidMoves[colour].Add(move);
                    }

                    // OK: The King is not in check, no need to evaluate if this is checkmate.
                    if (State[colour] == BoardState.NotInCheck) continue;

                    var king = colour == ChessPiece.EColour.Black ? BlackKing : WhiteKing;
                    Move(move, true);

                    // If this move would put the King out of check, then we know that they're 
                    // not in Checkmate.
                    if (!king.IsInCheck(Pieces)) {
                        State[colour] = BoardState.Check;
                    }
                    Undo();
                }
            }
        }

        // If we're not in checkmate and there are no valid moves left, this is a stalemate.
        foreach (ChessPiece.EColour colour in Enum.GetValues(typeof(ChessPiece.EColour))) {
            if (State[colour] == BoardState.Checkmate) return;
            if (ValidMoves[colour].Count == 0) State[colour] = BoardState.Stalemate;
        }
    }
    #endregion

    #region Private

    private bool WouldPutKingInCheck(ChessPiece piece, int toRow, int toColumn) {
        var king = piece.Colour == ChessPiece.EColour.Black ? BlackKing : WhiteKing;
        var move = new Move(piece.Row, piece.Column, toRow, toColumn);
        if (!Move(move, true)) {
            throw new System.Exception($"Attempted to make invalid move: {move}");
        }
        var isInCheck = (king.IsInCheck(Pieces));
        Undo();
        return isInCheck;
    }

    private bool PromotePiece(Move move, bool lazy) {
        // First, ensure this is actually valid.
        var (fromRow, fromColumn, toRow, toColumn) = (move.FromRow, move.FromColumn, move.ToRow, move.ToColumn);
        var pawn = Pieces[fromRow, fromColumn];

        if (pawn == null) {
            throw new Exception($"Attempted to make invalid promotion: {move} - piece is null. Board state: {this}");
        }

        if (move.PieceToPromoteTo == ChessPiece.EName.None) {
            throw new Exception($"Unable to promote piece with move {move} - PieceToPromoteTo is null. Board state: {this} ");
        }

        if (pawn.Name != ChessPiece.EName.Pawn) {
            throw new Exception($"Attempted to make invalid promotion: {move} with piece {pawn}. Board state: {this}");
        }

        var promotionRow = pawn.Colour == ChessPiece.EColour.Black ? 7 : 0;
        if (toRow != promotionRow) {
            throw new Exception($"Attempted to make invalid promotion: {move}. Row {toRow} is not {promotionRow}. Board state: {this}");
        }

        if (!pawn.CheckMove(Pieces, toRow, toColumn)) {
            throw new Exception($"Invalid prmotion {move} is not a valid move! Board state: {this}");
        }

        if (!lazy) {
            if (WouldPutKingInCheck(pawn, toRow, toColumn)) {
                throw new Exception($"Invalid promotion {move} would put king in check! Board state: {this}");
            }
        }

        // Phew. Okay, now we can do the promotion.
        // Kill the pawn.
        Pieces[fromRow, fromColumn] = null;

        // Create the piece.
        CreatePiece(move.PieceToPromoteTo, toRow, toColumn, pawn.Colour);

        // Do bookkeeping
        UndoStack.Push((move, pawn));

        if (!lazy) {
            CheckBoard();
        }

        Turn++;

        return true;
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
    private int GetCastlingDestination(int toColumn) {
        // FIDE 3.8 a. 
        // "..the king is transferred from its original square two squares towards the
        // rook on its original square"
        if (toColumn == 7) return 6;
        else return 2;
    }
    private void MoveRookForCastling(int toRow, int toColumn) {
        // FIDE 3.8 a. 
        // "..then that rook is transferred to the square the king has
        // just crossed."
        var rook = Pieces[toRow, toColumn];
        Pieces[toRow, toColumn] = null;

        if (toColumn == 7) toColumn = 5;
        else toColumn = 3;

        // Update the piece's bookkeeping.
        rook.Column = toColumn;

        // Update our own bookkeeping.
        Pieces[toRow, toColumn] = rook;
    }

    private void UndoCastle(int row, int kingMovedToColumn) {
        int originalColumn, rookMovedToColumn;
        if (kingMovedToColumn == 2) {
            originalColumn = 0;
            rookMovedToColumn = 3;
        } else {
            originalColumn = 7;
            rookMovedToColumn = 5;
        }

        var rook = Pieces[row, rookMovedToColumn];
        rook.Column = originalColumn;
        Pieces[row, rookMovedToColumn] = null;
        Pieces[row, originalColumn] = rook;
    }

    private void UndoPromotion(Move lastMove, ChessPiece pawn) {
        var (fromRow, fromColumn, toRow, toColumn) = (lastMove.FromRow, lastMove.FromColumn, lastMove.ToRow, lastMove.ToColumn);

        // Remove the "promotion" piece.
        Pieces[toRow, toColumn] = null;
        Pieces[fromRow, fromColumn] = pawn;

        // Done-ski desu~~
        Turn--;
    }

    #endregion // private

    #region Overrides
    public override string ToString() {
        var s = "";
        foreach (var p in Pieces) {
            if (p == null) continue;
            s += p.ToString();
            s += ", ";
        }
        return s;
    }
    #endregion

    public enum BoardState {
        NotInCheck,
        Check,
        Checkmate,
        Stalemate
    }
}