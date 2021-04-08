using System;

[Serializable]
public class ChessException : Exception {
    public ChessException() { }

    public ChessException(string message)
        : base(message) { }

    public ChessException(string message, Exception inner)
        : base(message, inner) { }
};