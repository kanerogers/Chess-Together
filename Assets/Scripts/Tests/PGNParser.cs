using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
public class PGNParser {
    StreamReader streamReader;
    ChessBoard board; // overkill? but seriously how else can I keep track of moves?
    public PGNParser(string filename) {
        streamReader = new StreamReader(filename);
        board = new ChessBoard();
    }

    static char BISHOP = 'B';
    static char KING = 'K';
    static char QUEEN = 'Q';
    static char KNIGHT = 'N';
    static char ROOK = 'R';
    static Dictionary<char, int> RANK_TO_ROW = new Dictionary<char, int>() {
        {'1', 7},
        {'2', 6},
        {'3', 5},
        {'4', 4},
        {'5', 3},
        {'6', 2},
        {'7', 1},
        {'8', 0},
    };
    static Dictionary<char, int> FILE_TO_COLUMN = new Dictionary<char, int>() {
        {'a', 7},
        {'b', 6},
        {'c', 5},
        {'d', 4},
        {'e', 3},
        {'f', 2},
        {'g', 1},
        {'h', 0},
    };
    Regex moveNumberRegex = new Regex(@"(^|\s)\d+\.", RegexOptions.Compiled);

    public List<Move> GetMoves() {
        var moves = new List<Move>();
        char[] MOVES_START = new char[] { '1', '.' };

        // First, we need to skip through the header stuff and get to the moves.
        while (true) {
            var line = streamReader.ReadLine();
            if (line.StartsWith("[") || line == "") {
                continue;
            }

            parseLine(line, moves);


            break;
        }

        return moves;
    }

    void parseLine(string line, List<Move> moves) {
        var matches = moveNumberRegex.Matches(line);
        var positions = new List<(int, int, int)>();
        foreach (Match match in matches) {
            var (index, length) = (match.Index, match.Length);
            if (positions.Count == 0) {
                positions.Add((index, length, 0));
                continue;
            }

            var (_, lastEnd, l) = positions[positions.Count - 1];
            positions.Add((lastEnd + l, index, length));
        }

        foreach (var (i, l, _) in positions) {
            if (i == 0) continue;
            var m = line.Substring(i, (l - i) + 1);
            var split = m.Split(' ');
            var whiteMoveString = split[0];
            var blackMoveString = split[1];

            var whiteMove = parseMoveString(whiteMoveString, ChessPiece.EColour.White);
            if (!board.Move(whiteMove)) throw new System.Exception($"Attempted to make invalid move: {whiteMove}");
            moves.Add(whiteMove);

            var blackMove = parseMoveString(whiteMoveString, ChessPiece.EColour.Black);
            if (!board.Move(blackMove)) throw new System.Exception($"Attempted to make invalid move: {blackMove}");
            moves.Add(blackMove);
        }
    }

    public Move parseMoveString(string moveString, ChessPiece.EColour colour) {
        // First, find the piece's name based off the move string
        var (pieceName, coordinateString) = parseName(moveString);

        // Then convert the coordinates to our format (fromRow, fromColumn, toRow, toColumn)
        // Noting that startRow and startColumn will usually be -1 indicating they're not present.
        // They will only be used if necessary to disambiguate
        var (fromRow, fromColumn, toRow, toColumn) = parseCoordinates(coordinateString);

        // Then look through ValidMoves[Colour] for moves with matching toRow, toColumn
        var moveCandidates = board.ValidMoves[colour].FindAll(m => m.ToRow == toRow && m.ToColumn == toColumn);

        // If moves.Count == 1, return that move
        if (moveCandidates.Count == 1) return moveCandidates[0];

        // Else, look at the piece on startRow, startColumn and check if its name is pieceName, or use
        // startRow or startColumn provided in the moveString.
        foreach (var candidate in moveCandidates) {
            var (row, column) = (candidate.FromRow, candidate.FromColumn);
            var piece = board.Pieces[row, column];
            if (piece.Name == pieceName) return candidate;
        }

        return null;
    }

    private (int, int, int, int) parseCoordinates(string coordinateString) {
        int fromRow = -1, fromColumn = -1, toRow, toColumn;
        if (coordinateString.Length == 2) {
            var rank = coordinateString[1]; // row
            var file = coordinateString[0]; // column
            toRow = RANK_TO_ROW[rank];
            toColumn = FILE_TO_COLUMN[file];
            return (fromRow, fromColumn, toRow, toColumn);
        }

        Logger.Log("PGNParser", $"Found unknown string {coordinateString}");

        throw new System.NotImplementedException();
    }

    private (ChessPiece.EName, string) parseName(string moveString) {
        var c = moveString[0];

        if (!System.Char.IsUpper(c)) return (ChessPiece.EName.Pawn, moveString);
        var coordinateString = moveString.Substring(1, moveString.Length - 1);
        if (c == KNIGHT) return (ChessPiece.EName.Knight, coordinateString);
        if (c == BISHOP) return (ChessPiece.EName.Bishop, coordinateString);
        if (c == KING) return (ChessPiece.EName.King, coordinateString);
        if (c == QUEEN) return (ChessPiece.EName.Queen, coordinateString);
        if (c == ROOK) return (ChessPiece.EName.Rook, coordinateString);

        // TODO: Castling happens, need to deal with that.
        throw new System.Exception($"Invalid character: {c}");
    }
}