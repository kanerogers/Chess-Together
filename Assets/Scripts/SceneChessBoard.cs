using System.Collections;
using System;
using UnityEngine;
using PathologicalGames;

public class SceneChessBoard : MonoBehaviour {
    static float left = -0.25f;
    static float top = 0.25f;
    static float space = 0.0625f;
    static float piecePadding = space / 2;
    public float MovementDuration = 0.5f;
    const string POOL_NAME = "Pieces";
    public AnimationCurve PieceMoveCurve;
    public GameObject SquarePrefab;
    public Material Black;
    public Material White;
    public SceneChessPiece[,] Pieces = new SceneChessPiece[8, 8];
    public ChessBoard LogicBoard;
    public GameManager GameManager;
    public bool bigPieces = false;
    public float scale = 1f;
    public Transform Pivot = null;
    public SceneChessPiece bishopPrefab;
    public SceneChessPiece kingPrefab;
    public SceneChessPiece queenPrefab;
    public SceneChessPiece pawnPrefab;
    public SceneChessPiece rookPrefab;
    public SceneChessPiece knightPrefab;
    public BoardInterfaceManager boardInterfaceManager;
    bool initialized = false;
    Move promotionMove = null;
    GameObject[] Squares = new GameObject[64];

    public void InitializeBoard(ChessBoard board) {
        if (initialized) {
            ReinitializeBoard(board);
            return;
        }

        if (bigPieces) {
            transform.localScale *= scale;
        }

        LogicBoard = board;

        SetUpBoard();
        SetUpSquares();
        initialized = true;
    }

    public void ReinitializeBoard(ChessBoard board) {
        DestroyBoard();
        LogicBoard = board;
        SetUpBoard();
    }

    public void DestroyBoard() {
        foreach (var piece in Pieces) {
            if (!piece || !piece.gameObject.activeSelf) continue;
            PoolManager.Pools[POOL_NAME].Despawn(piece.transform);
        }
        Pieces = new SceneChessPiece[8, 8];
    }

    // Move a piece to some arbitrary place on the board
    public bool Move(SceneChessPiece scenePiece, int toRow, int toColumn) {
        var fromRow = scenePiece.Piece.Row;
        var fromColumn = scenePiece.Piece.Column;
        var move = new Move(fromRow, fromColumn, toRow, toColumn);
        return Move(scenePiece, move);
    }

    public bool Move(SceneChessPiece scenePiece, Move move) {
        var (fromRow, fromColumn, toRow, toColumn) = move.ToCoordinates();

        // If this move is a promotion we need to do some special handling.
        if (scenePiece.Piece.Name == ChessPiece.EName.Pawn) {
            var pawn = (Pawn)scenePiece.Piece;
            if (pawn.IsPromotion(toRow, toColumn)) {
                promotionMove = move;
                var p = CoordinatesForPosition(toRow, toColumn);
                scenePiece.Move(p, false); // move the piece to the end row while we choose the promotion piece
                // necessary to pass false here to not trigger MoveCompleted.

                boardInterfaceManager.AskForPieceToPromoteTo();
                return true;
            }
        }


        // Will take care of all business logic.
        if (!LogicBoard.Move(move)) return false;
        if (move.IsCastling) {
            MoveCastle(scenePiece, move);
            return true;
        }

        // This move is valid. Proceed!
        EventManager.DeselectedPiece(fromRow, fromColumn);

        var removedPiece = getRemovedPiece(move);
        if (removedPiece) {
            if (removedPiece.Piece.Colour == scenePiece.Piece.Colour) {
                var pgn = PGNExporter.ToPGN(LogicBoard);
                Logger.Log(pgn);
                throw new ChessException($"{move} is invalid! Can't remove a piece of the same colour!");
            }
            StartCoroutine(RemovePiece(removedPiece.gameObject, scenePiece.MovementDuration));
        }

        // Update our internal bookkeeping
        Pieces[fromRow, fromColumn] = null;
        Pieces[toRow, toColumn] = scenePiece;

        for (int r = 0; r != 8; r++) {
            for (int c = 0; c != 8; c++) {
                var sp = Pieces[r, c];
                if (sp == null) continue;
                var piece = sp.Piece;

                if (piece.Row != r || piece.Column != c) {
                    // This piece has fallen out of sync. This can happen because the AI may make moves that result
                    // in the creation/destruction of pieces. In this case, we'll just look to the LogicBoard as the source
                    // of truth.
                    var logicBoardPiece = LogicBoard.Pieces[r, c];
                    if (logicBoardPiece.Name != piece.Name || logicBoardPiece.Colour != piece.Colour) {
                        throw new ChessException($"Invalid state! {logicBoardPiece} != {piece}");
                    }
                    sp.Piece = logicBoardPiece;
                }

                if (LogicBoard.Pieces[r, c] == null) {
                    throw new ChessException($"{piece} has been removed but we are still holding a reference to it!");
                }
            }
        }

        // Move the piece in the Scene
        var position = CoordinatesForPosition(toRow, toColumn);
        scenePiece.Move(position);

        return true;
    }


    // Move a piece to the location of another piece
    public bool Move(SceneChessPiece scenePiece, SceneChessPiece otherPiece) {
        var toRow = otherPiece.Piece.Row;
        var toColumn = otherPiece.Piece.Column;

        // Handle the move
        if (!Move(scenePiece, toRow, toColumn)) return false;

        return true;
    }

    // Make a move generated by the AI
    public bool Move(Move move) {
        var (fromRow, fromColumn, toRow, toColumn) = move.ToCoordinates();
        var scenePiece = Pieces[fromRow, fromColumn];
        if (!scenePiece) {
            var pgn = PGNExporter.ToPGN(LogicBoard);
            Logger.LogError(pgn);
            throw new ChessException($"No piece found at {move.FromRow},{move.FromColumn}");
        }

        if (move.IsPromotion()) {
            StartCoroutine(AIPromotion(scenePiece, move));
            return true;
        }

        if (!Move(scenePiece, move)) {
            throw new ChessException($"Move {move} is invalid!");
        }

        return true;
    }

    private IEnumerator AIPromotion(SceneChessPiece scenePiece, Move move) {
        promotionMove = move;
        var p = CoordinatesForPosition(move.ToRow, move.ToColumn);
        scenePiece.Move(p, false); // move the piece to the end row while we choose the promotion piece
        yield return new WaitForSeconds(MovementDuration + 0.1f);

        PromotePawnTo(move.PieceToPromoteTo);
        yield return null;
    }

    public Vector3 CoordinatesForPosition(int toRow, int toColumn) {
        float x = left + (toColumn * space) + piecePadding;
        float z = top - (toRow * space) - piecePadding;
        return new Vector3(x, 0, z) * scale;
    }

    public (int row, int column) GetPositionForCoordinates(Vector3 point) {
        point /= scale;
        int row = Mathf.FloorToInt((top - point.z) / space);
        var col = Math.Abs(Mathf.CeilToInt((left - point.x) / space));

        return (row, col);
    }

    private void MoveCastle(SceneChessPiece king, Move move) {
        var (fromRow, fromColumn, toRow, toColumn) = move.ToCoordinates();
        EventManager.DeselectedPiece(fromRow, fromColumn);

        var rookFromColumn = toColumn == 6 ? 7 : 0;
        var rookToColumn = toColumn == 6 ? 5 : 3;

        // Grab the rook
        var rook = Pieces[toRow, rookFromColumn];

        // Update our internal bookkeeping

        // Clear the King
        Pieces[fromRow, fromColumn] = null;

        // Clear the rook
        Pieces[fromRow, rookFromColumn] = null;

        Pieces[fromRow, toColumn] = king;
        Pieces[fromRow, rookToColumn] = rook;

        // Move the piece in the Scene
        var kingPosition = CoordinatesForPosition(toRow, toColumn);
        king.Move(kingPosition);

        var rookPosition = CoordinatesForPosition(toRow, rookToColumn);

        // Tell the rook that this move is invalid so it doesn't trigger TurnComplete.
        rook.Move(rookPosition, false);
    }

    public void SetUpBoard() {
        foreach (var piece in LogicBoard.Pieces) {
            if (piece == null) continue;
            CreatePiece(piece);
        }
    }

    public void PromotePawnTo(ChessPiece.EName pieceToPromoteTo) {
        // First, tell the logic board what we're up to.
        var move = promotionMove;
        var (fromRow, fromColumn, toRow, toColumn) = move.ToCoordinates();
        var scenePiece = Pieces[fromRow, fromColumn];

        Logger.Log($"PROMOTING PAWN: {scenePiece.Piece}");
        move.PieceToPromoteTo = pieceToPromoteTo;

        Logger.Log($"PROMOTION MOVE: {move}");
        if (!LogicBoard.Move(move)) {
            throw new ChessException($"Promotion invalid: {move}");
        }

        // Now replace the piece.
        PoolManager.Pools[POOL_NAME].Despawn(Pieces[fromRow, fromColumn].transform);

        // Null out the reference
        Pieces[fromRow, fromColumn] = null;
        var promotedPiece = LogicBoard.Pieces[toRow, toColumn];
        CreatePiece(promotedPiece);

        // And signal that a move has been completed.
        EventManager.EndMove(scenePiece.Piece.Colour);

        // ..and that should be that! Probably.
    }

    void CreatePiece(ChessPiece piece) {
        var row = piece.Row;
        var column = piece.Column;
        var colour = piece.Colour;
        var name = piece.Name.ToString();
        GameObject obj;

        if (name == "Bishop") {
            var t = PoolManager.Pools[POOL_NAME].Spawn(bishopPrefab.transform);
            obj = t.gameObject;
        } else if (name == "King") {
            var t = PoolManager.Pools[POOL_NAME].Spawn(kingPrefab.transform);
            obj = t.gameObject;
        } else if (name == "Knight") {
            var t = PoolManager.Pools[POOL_NAME].Spawn(knightPrefab.transform);
            obj = t.gameObject;
        } else if (name == "Pawn") {
            var t = PoolManager.Pools[POOL_NAME].Spawn(pawnPrefab.transform);
            obj = t.gameObject;
        } else if (name == "Rook") {
            var t = PoolManager.Pools[POOL_NAME].Spawn(rookPrefab.transform);
            obj = t.gameObject;
        } else if (name == "Queen") {
            var t = PoolManager.Pools[POOL_NAME].Spawn(queenPrefab.transform);
            obj = t.gameObject;
        } else {
            throw new ChessException("invalid piece name");
        }

        SceneChessPiece sp = obj.GetComponent<SceneChessPiece>();
        sp.gameManager = GameManager;
        sp.Board = this;
        sp.MoveCurve = PieceMoveCurve;
        obj.transform.parent = Pivot;
        obj.transform.localPosition = CoordinatesForPosition(row, column);
        obj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        if (name == "Knight" && colour == ChessPiece.EColour.Black) {
            obj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        if (bigPieces) {
            obj.transform.localScale *= scale;
        }

        var renderer = obj.GetComponent<Renderer>();
        Material material;

        if (colour == ChessPiece.EColour.Black) {
            material = Black;
        } else {
            material = White;
        }


        renderer.material = material;
        sp.Piece = piece;
        Pieces[row, column] = sp;
    }

    void CreateSquare(int row, int column) {
        var position = CoordinatesForPosition(row, column);
        var t = PoolManager.Pools[POOL_NAME].Spawn(SquarePrefab);
        var obj = t.gameObject;
        obj.transform.parent = Pivot;
        obj.transform.localPosition = position;

        var square = obj.GetComponent<Square>();
        square.Row = row;
        square.Column = column;

        if (bigPieces) {
            obj.transform.localScale *= scale;
        }
    }

    IEnumerator RemovePiece(GameObject obj, float movementDuration) {
        Logger.Log($"Removing {obj.GetComponent<SceneChessPiece>().Piece}");
        yield return new WaitForSeconds(movementDuration);
        PoolManager.Pools[POOL_NAME].Despawn(obj.transform);
    }
    void SetUpSquares() {
        for (int row = 0, i = 0; row < 8; row++) {
            for (int column = 0; column < 8; column++, i++) {
                CreateSquare(row, column);
            }
        }
    }

    private SceneChessPiece getRemovedPiece(Move move) {
        if (move.IsEnPassantCapture) {
            var (_, removedPiece) = LogicBoard.UndoStack.Peek();
            var (row, column) = (removedPiece.Row, removedPiece.Column);
            var p = Pieces[row, column];

            // We also need to set it the square to null, as the normal logic doesn't handle EnPassant captures.
            Pieces[row, column] = null;
            return p;
        }

        return Pieces[move.ToRow, move.ToColumn];
    }
}
