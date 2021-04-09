public static class EventManager {
    public delegate void MoveCompleteHandler(ChessPiece.EColour justMoved);
    public delegate void RowHandler(int row, int column);
    public static event MoveCompleteHandler MoveComplete;
    public static event RowHandler SquareHovered;
    public static event RowHandler SquareUnhovered;
    public static event RowHandler PieceHovered;
    public static event RowHandler PieceUnhovered;
    public static event RowHandler PieceSelected;
    public static event RowHandler PieceDeselected;
    public static void EndMove(ChessPiece.EColour justMoved) {
        MoveComplete?.Invoke(justMoved);
    }

    public static void HoveredOverSquare(int row, int column) => SquareHovered?.Invoke(row, column);
    public static void UnhoveredOverSquare(int row, int column) => SquareUnhovered?.Invoke(row, column);
    public static void HoveredOverPiece(int row, int column) => PieceHovered?.Invoke(row, column);
    public static void UnhoveredOverPiece(int row, int column) => PieceUnhovered?.Invoke(row, column);
    public static void SelectedPiece(int row, int column) => PieceSelected?.Invoke(row, column);
    public static void DeselectedPiece(int row, int column) => PieceDeselected?.Invoke(row, column);
}