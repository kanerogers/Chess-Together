public static class Extensions {
    public static ChessPiece.EColour Inverse(this ChessPiece.EColour colour) {
        return colour == ChessPiece.EColour.Black ? ChessPiece.EColour.White : ChessPiece.EColour.Black;
    }

    public static bool IsBlack(this ChessPiece.EColour colour) {
        return colour == ChessPiece.EColour.Black;
    }
    public static bool IsWhite(this ChessPiece.EColour colour) {
        return colour == ChessPiece.EColour.White;
    }
}