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
        UpdateBoardState();
    }

    public ChessBoard(bool emptyBoard) {
        Inst = this;
        ValidMoves[ChessPiece.EColour.Black] = new List<Move>();
        ValidMoves[ChessPiece.EColour.White] = new List<Move>();

        if (emptyBoard) return;
        SetUpBoard();

        State[ChessPiece.EColour.Black] = BoardState.NotInCheck;
        State[ChessPiece.EColour.White] = BoardState.NotInCheck;
        UpdateBoardState();
    }

    public bool Move(Move move, bool lazy = false) {
        // Is this move valid?
        var (fromRow, fromColumn, toRow, toColumn) = move.ToCoordinates();
        var piece = Pieces[fromRow, fromColumn];

        // Does the piece exist?
        if (piece == null) return false;

        // Is this a valid move?
        if (!piece.CheckMove(Pieces, move)) return false;

        // Would this put us in check?
        if (!lazy) if (WouldPutKingInCheck(move)) return false;

        // Move is valid! 

        // Before we can update the state, we need to stash the move away.
        var removedPiece = Pieces[toRow, toColumn];
        UndoStack.Push((move, removedPiece));

        // Update state.
        UpdatePiecesState(move);            // Updates Pieces[]
        UpdateTurn(move);                   // Updates Turn 
        if (!lazy) UpdateBoardState();      // Updates BoardState and ValidMoves 
        UpdateEnPassantState(move);         // Updates EnPassantState

        // Done!
        return true;

    }

    public bool Move(int fromRow, int fromColumn, int toRow, int toColumn, bool lazy = false) {
        var move = new Move(fromRow, fromColumn, toRow, toColumn);
        return Move(move, lazy);
    }

    public bool IsValidMove(ChessPiece piece, int toRow, int toColumn) {
        var validMoves = ValidMoves[piece.Colour];
        var move = new Move(piece.Row, piece.Column, toRow, toColumn);

        return validMoves.Contains(move);
    }

    public void Undo() {
        var (lastMove, removedPiece) = UndoStack.Pop();
        Logger.Log("UNDO", $"Undoing {lastMove}");
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

        // Clear the EnPassant state - it's now invalid.
        ClearEnPassantState();

        // Restore any en passant state
        if (lastMove.previousEnPassantState != null) {
            var (row, column) = (lastMove.previousEnPassantState.Row, lastMove.previousEnPassantState.Column);
            Logger.Log($"Restoring EnPassant state for {row},{column} = {lastMove.previousEnPassantState.CanBeCaptured}");
            EnPassantState[(row, column)] = lastMove.previousEnPassantState.CanBeCaptured;
        }

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
    public void UpdateBoardState() {
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
    private void UpdatePiecesState(Move move) {
        if (move.PieceToPromoteTo != ChessPiece.EName.None) {
            PromotePiece(move);
            return;
        }

        var (fromRow, fromColumn, toRow, toColumn) = move.ToCoordinates();
        var piece = Pieces[toRow, toColumn];

        // Update our own bookkeeping.
        Pieces[toRow, toColumn] = piece;
        Pieces[fromRow, fromColumn] = null;
    }

    private void UpdateTurn(Move move) {
        // TODO: Set turn in move?
        Turn++;
    }

    private void UpdateEnPassantState(Move move) {
    }


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

    private bool WouldPutKingInCheck(Move move) {
        var (fromRow, fromColumn, _, _) = move.ToCoordinates();
        var piece = Pieces[fromRow, fromColumn];
        var king = piece.Colour == ChessPiece.EColour.Black ? BlackKing : WhiteKing;

        if (!Move(move, true)) {
            throw new System.Exception($"Attempted to make invalid move: {move}");
        }

        var isInCheck = (king.IsInCheck(Pieces));
        Undo();
        return isInCheck;
    }

    private void PromotePiece(Move move) {
        var (fromRow, fromColumn, toRow, toColumn) = move.ToCoordinates();
        var pawn = Pieces[fromRow, fromColumn];

        // Phew. Okay, now we can do the promotion.
        // Kill the pawn.
        Pieces[fromRow, fromColumn] = null;

        // Create the piece.
        CreatePiece(move.PieceToPromoteTo, toRow, toColumn, pawn.Colour);

        // Modify the undo stack so that it's actually the *pawn* that's the removed piece.
        var (_, piece) = UndoStack.Peek();
        piece = pawn;
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
    void ClearEnPassantState() {
        foreach (var s in EnPassantState) {
            var (row, column) = s.Key;
            var pawn = Pieces[row, column];
            Logger.Log($"Setting new EnPassant state for {row},{column} = false");
            if (pawn != null) {
                if (pawn.Name != ChessPiece.EName.Pawn) {
                    throw new Exception($"Invalid enpassant state - {pawn}");
                }
                var p = (Pawn)pawn;
                p.CanBeCapturedByEnpassant = false;
            }
        }

        EnPassantState.Clear();

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