public class PreviousEnPassantState {
    public int Row;
    public int Column;
    public bool CanBeCaptured;

    public PreviousEnPassantState(int row, int column, bool canBeCaptured) {
        Row = row;
        Column = column;
        CanBeCaptured = canBeCaptured;
    }
}