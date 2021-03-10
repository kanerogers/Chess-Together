﻿using System.Collections;
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
    public GameObject[,] Pieces = new GameObject[8, 8];
    public ChessBoard LogicBoard;
    public GameManager GameManager;
    public bool bigPieces = true;
    public float scale = 5f;
    public Transform Pivot = null;
    public SceneChessPiece bishopPrefab;
    public SceneChessPiece kingPrefab;
    public SceneChessPiece queenPrefab;
    public SceneChessPiece pawnPrefab;
    public SceneChessPiece rookPrefab;
    public SceneChessPiece knightPrefab;
    public BoardInterfaceManager boardInterfaceManager;
    bool initialized = false;
    (SceneChessPiece, int, int) promotionData = (null, 0, 0);
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
            if (!piece || !piece.activeSelf) continue;
            PoolManager.Pools[POOL_NAME].Despawn(piece.transform);
        }
        Pieces = new GameObject[8, 8];
    }

    // Move a piece to some arbitrary place on the board
    public bool Move(SceneChessPiece scenePiece, int toRow, int toColumn) {
        var fromRow = scenePiece.Piece.Row;
        var fromColumn = scenePiece.Piece.Column;

        // If this move is a promotion we need to do some special handling.
        if (scenePiece.Piece.Name == ChessPiece.EName.Pawn) {
            var pawn = (Pawn)scenePiece.Piece;
            if (pawn.IsPromotion(toRow, toColumn)) {
                promotionData = (scenePiece, toRow, toColumn);
                var p = CoordinatesForPosition(toRow, toColumn);
                scenePiece.Move(p, false); // move the piece to the end row while we choose the promotion piece
                // necessary to pass false here to not trigger MoveCompleted.

                boardInterfaceManager.AskForPieceToPromoteTo();
                return true;
            }
        }

        // Will take care of all business logic.
        if (!LogicBoard.Move(fromRow, fromColumn, toRow, toColumn)) return false;
        if (IsCastling()) {
            MoveCastle(scenePiece, toRow, toColumn);
            return true;
        }

        // This move is valid. Proceed!
        EventManager.DeselectedPiece(fromRow, fromColumn);

        var otherPiece = Pieces[toRow, toColumn];
        if (otherPiece) {
            StartCoroutine(RemovePiece(otherPiece.gameObject, scenePiece.MovementDuration));
        }

        // Update our internal bookkeeping
        Pieces[fromRow, fromColumn] = null;
        Pieces[toRow, toColumn] = scenePiece.gameObject;

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
        var obj = Pieces[move.FromRow, move.FromColumn];
        if (!obj) {
            throw new Exception($"No piece found at {move.FromRow},{move.FromColumn}");
        }

        var scenePiece = obj.GetComponent<SceneChessPiece>();
        return Move(scenePiece, move.ToRow, move.ToColumn);
    }

    public Vector3 CoordinatesForPosition(int toRow, int toColumn) {
        float x = left + (toColumn * space) + piecePadding;
        float z = top - (toRow * space) - piecePadding;
        return new Vector3(x, 0, z) * scale;
    }
    private void MoveCastle(SceneChessPiece king, int toRow, int toColumn) {
        var (fromRow, fromColumn) = (king.Piece.Row, king.Piece.Column);
        EventManager.DeselectedPiece(fromRow, fromColumn);

        var rook = Pieces[toRow, toColumn].GetComponent<SceneChessPiece>();

        // Update our internal bookkeeping
        Pieces[fromRow, fromColumn] = null;
        Pieces[toRow, toColumn] = null;

        var kingColumn = toColumn == 7 ? 6 : 2;
        var rookColumn = toColumn == 7 ? 5 : 3;
        Pieces[fromRow, kingColumn] = king.gameObject;
        Pieces[fromRow, rookColumn] = rook.gameObject;

        // Move the piece in the Scene
        var kingPosition = CoordinatesForPosition(toRow, kingColumn);
        king.Move(kingPosition);

        var rookPosition = CoordinatesForPosition(toRow, rookColumn);
        rook.Move(rookPosition);
    }

    public void SetUpBoard() {
        foreach (var piece in LogicBoard.Pieces) {
            if (piece == null) continue;
            CreatePiece(piece);
        }
    }

    public void PromotePawnTo(ChessPiece.EName name) {
        // First, tell the logic board what we're up to.
        var (scenePiece, toRow, toColumn) = promotionData;
        Debug.Log($"PROMOTING PAWN: {scenePiece.Piece}");
        var (fromRow, fromColumn) = (scenePiece.Piece.Row, scenePiece.Piece.Column);
        var move = new Move(fromRow, fromColumn, toRow, toColumn, name);
        Debug.Log($"PROMOTION MOVE: {move}");
        if (!LogicBoard.Move(move)) {
            Debug.LogError($"Promotion invalid: {move}");
        }

        // Now replace the piece.
        PoolManager.Pools[POOL_NAME].Despawn(Pieces[fromRow, fromColumn].transform);
        var promotedPiece = LogicBoard.Pieces[toRow, toColumn];
        CreatePiece(promotedPiece);

        // And signal that a move has been completed.
        EventManager.EndMove();

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
            throw new Exception("invalid piece name");
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
        Pieces[row, column] = obj;
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

    private bool IsCastling() => LogicBoard.UndoStack.Peek().Item1.IsCastling;

    IEnumerator RemovePiece(GameObject obj, float movementDuration) {
        Debug.Log($"Removing {obj.GetComponent<SceneChessPiece>().Piece}");
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
}
