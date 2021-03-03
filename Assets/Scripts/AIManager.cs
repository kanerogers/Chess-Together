using System.Collections.Generic;
using UnityEngine;

public static class AIManager {
    public enum MoveType {
        Opening,
        Defence,
        Standard,
    }

    static int CHECKMATE_MOVE = 100;
    static int CHECK_MOVE = 10;
    static int MINMAX_DEPTH = 3;
    public static Move GetMove(ChessBoard board, ChessPiece.EColour AI, MoveType moveType) {
        if (moveType == MoveType.Opening) return GetOpeningMove(AI);
        if (moveType == MoveType.Defence) return GetDefenceMove(AI);

        int bestScore = -99999999;
        var validMoves = new List<Move>(board.ValidMoves[AI]);
        if (validMoves.Count == 0) {
            throw new System.Exception($"Wuh woh, no valid moves left. Board state is {board}");
        }

        foreach (Move move in validMoves) {
            int depthReached;
            (move.Score, depthReached) = MinMax(board, move, AI, AI.Inverse(), AI, MINMAX_DEPTH);

            for (int i = 0; i < depthReached; i++) {
                board.Undo();
            }

            if (move.Score > bestScore) {
                bestScore = move.Score;
            }
        }

        var bestMoves = validMoves.FindAll(m => m.Score == bestScore);
        if (bestMoves.Count == 0) {
            throw new System.Exception($"Somehow, some way, bestMoves.Count == 0. But valid moves.Count == {validMoves.Count}");
        }

        var random = new System.Random();
        var chosenMove = bestMoves[random.Next(bestMoves.Count - 1)];

        return chosenMove;
    }

    private static Move GetDefenceMove(ChessPiece.EColour AI) {
        var moves = new Dictionary<ChessPiece.EColour, Move[]>()
        {
            { ChessPiece.EColour.Black, new Move[] {
                new Move(1, 4, 3, 4)
            }},
            { ChessPiece.EColour.White, new Move[] {
                new Move(6, 3, 4, 3)
            }}
        };

        var random = new System.Random();
        return (Move)moves[AI].GetValue(random.Next(moves[AI].Length));
    }

    private static Move GetOpeningMove(ChessPiece.EColour AI) {
        var moves = new Dictionary<ChessPiece.EColour, Move[]>()
        {
            { ChessPiece.EColour.Black, new Move[] {
                new Move(1, 3, 3, 3)
            }},
            { ChessPiece.EColour.White, new Move[] {
                new Move(6, 4, 4, 4)
            }}
        };

        var random = new System.Random();
        return (Move)moves[AI].GetValue(random.Next(moves[AI].Length));
    }

    static (int, int) MinMax(ChessBoard board, Move move, ChessPiece.EColour us, ChessPiece.EColour them, ChessPiece.EColour canMove, int depth = 0, int depthReached = 0) {
        // Bottom layer of MinMax:
        if (depth == 0) {
            var score = EvaluateMove(board, move, us, them, canMove);
            board.Undo();
            return (score, depthReached);
        }

        if (!board.Move(move)) {
            throw new System.Exception($"Attempted to make invalid move {move} with board state {board}.");
        }

        var ourState = board.State[us];
        if (ourState == ChessBoard.BoardState.Checkmate) {
            // Logger.Log("AI", $"Move {move} would put us into checkmate");
            board.Undo();
            return (-CHECKMATE_MOVE, depthReached);
        }

        var enemyState = board.State[them];
        if (enemyState == ChessBoard.BoardState.Checkmate) {
            // Logger.Log("AI", $"Move {move} would put the enemy into checkmate: {board}");
            board.Undo();
            return (CHECKMATE_MOVE, depthReached);
        }

        canMove = canMove.Inverse();

        // Are there any valid moves left?
        var validMoves = new List<Move>(board.ValidMoves[canMove]);
        if (validMoves.Count == 0) {
            // Logger.Log("AI", $"No valid moves for {canMove}");

            // No further valid moves, return current one
            if (canMove == us) {
                board.Undo();
                return (-CHECKMATE_MOVE, depthReached);
            } else {
                board.Undo();
                return (CHECKMATE_MOVE, depthReached);
            }
        }

        // Find best move.
        Move bestMove = null;
        foreach (Move moveToEvaluate in validMoves) {
            moveToEvaluate.Score = EvaluateMove(board, moveToEvaluate, us, them, canMove);
            board.Undo();
            if (bestMove == null) bestMove = moveToEvaluate;
            if (canMove == them) {
                moveToEvaluate.Score *= -1;
            }
            if (moveToEvaluate.Score > bestMove.Score) {
                bestMove = moveToEvaluate;
            }
        }

        return MinMax(board, bestMove, us, them, canMove, depth - 1, depthReached + 1);
    }

    static int EvaluateMove(ChessBoard board, Move move, ChessPiece.EColour us, ChessPiece.EColour them, ChessPiece.EColour canMove) {
        if (!board.Move(move)) {
            throw new System.Exception($"Attempted to make invalid move {move} with board state {board}");
        }

        var ourState = board.State[us];
        if (ourState == ChessBoard.BoardState.Check) {
            return -CHECK_MOVE;
        }
        if (ourState == ChessBoard.BoardState.Checkmate) {
            return -CHECKMATE_MOVE;
        }

        var enemyState = board.State[them];
        if (enemyState == ChessBoard.BoardState.Checkmate) {
            return CHECKMATE_MOVE;
        }

        int score = 0;
        foreach (var p in board.Pieces) {
            if (p == null) continue;
            var value = p.Colour == us ? p.GetScore() : -p.GetScore();
            score += value;
        }

        return score;
    }
}
