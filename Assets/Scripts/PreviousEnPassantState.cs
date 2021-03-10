public class PreviousEnPassantState {
    public Pawn Pawn;
    public bool CanBeCaptured;

    public PreviousEnPassantState(Pawn pawn, bool canBeCaptured) {
        Pawn = pawn;
        CanBeCaptured = canBeCaptured;
    }
}