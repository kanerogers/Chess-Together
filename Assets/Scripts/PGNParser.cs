using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
public class PGNParser {
    StreamReader streamReader;
    public ChessBoard Board;
    public PGNParser(string filename) {
        streamReader = new StreamReader(filename);
        Board = new ChessBoard();
    }

    public PGNParser() {
        Board = new ChessBoard();
    }

    static char BISHOP = 'B';
    static char KING = 'K';
    static char QUEEN = 'Q';
    static char KNIGHT = 'N';
    static char ROOK = 'R';
    static char CAPTURE = 'x';
    static char CHECK = '+';
    static string PROMOTION = "=";
    public static string QUEENSIDE_CASTLE = "O-O-O";
    public static string KINGSIDE_CASTLE = "O-O";
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
        {'a', 0},
        {'b', 1},
        {'c', 2},
        {'d', 3},
        {'e', 4},
        {'f', 5},
        {'g', 6},
        {'h', 7},
    };
    static Regex moveNumberRegex = new Regex(@"(^|\s)\d+(\.|-|\/)", RegexOptions.Compiled);
    static Regex scoreRegex = new Regex(@"\d\-\d", RegexOptions.Compiled);
    int lineNumber = 0;

    public List<Move> ParseSingleLine(string line) {
        var moves = new List<Move>();
        parseLine(line, moves);
        return moves;
    }

    public List<List<Move>> Parse(int numGames = 0) {
        var inHeader = false;
        var games = new List<List<Move>>();
        List<Move> moves = null;

        // First, we need to skip through the header stuff and get to the moves.
        while (true) {
            lineNumber++;
            var line = streamReader.ReadLine();
            if (line == null) {
                games.Add(moves);
                break;
            }
            if (line.StartsWith("[") || line == "") {
                if (inHeader) continue;
                else {
                    inHeader = true;
                    if (numGames != 0 && games.Count >= numGames) {
                        break;
                    }
                    Board = new ChessBoard();
                    // Add previous list
                    if (moves != null) games.Add(moves);

                    // Create a new one
                    moves = new List<Move>();

                    // Add the new one in
                    games.Add(moves);
                }
            } else {
                inHeader = false;

                try {
                    parseLine(line, moves);
                } catch (System.Exception e) {
                    Logger.Log("ERROR", $"Error parsing on line {lineNumber}: {line}.");
                    Logger.Log("ERROR", $"Board state is {Board}");
                    Logger.Log("ERROR", $"Valid moves are:");
                    foreach (var move in Board.ValidMoves[ChessPiece.EColour.Black]) {
                        Logger.Log("ERROR", move.ToString());
                    }
                    Logger.Log("ERROR", e.ToString());
                    throw e;
                }
            }

        }

        return games;
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

        if (!scoreRegex.IsMatch(line)) {
            var (_, le, lastLength) = positions[positions.Count - 1];
            positions.Add((le + lastLength, line.Length, 0));
        }


        foreach (var (i, l, _) in positions) {
            if (i == 0) continue;
            var m = line.Substring(i, (l - i));
            var split = m.Split(' ');
            var whiteMoveString = split[0];
            string blackMoveString = "";
            if (split.Length == 2) {
                blackMoveString = split[1];
            }

            var whiteMove = parseMoveString(whiteMoveString, ChessPiece.EColour.White);
            // Logger.Log("PGN", $"{whiteMoveString} is {whiteMove}");
            if (!Board.Move(whiteMove)) throw new System.Exception($"Attempted to make invalid move: {whiteMove}");
            moves.Add(whiteMove);

            // If white won, then black's final move will be empty.
            if (blackMoveString == "") return;

            var blackMove = parseMoveString(blackMoveString, ChessPiece.EColour.Black);
            // Logger.Log("PGN", $"{blackMoveString} is {blackMove}");
            if (!Board.Move(blackMove)) throw new System.Exception($"Attempted to make invalid move: {blackMove}");
            moves.Add(blackMove);
        }
    }

    public Move parseMoveString(string moveString, ChessPiece.EColour colour) {
        // Logger.Log("PGN", $"Parsing {moveString} for {colour}");

        // Remove noise
        moveString = moveString.Replace(CHECK.ToString(), "");
        moveString = moveString.Replace(CAPTURE.ToString(), "");

        // If this is a Castle, then handle that
        if (moveString == QUEENSIDE_CASTLE) return parseQueensideCastle(moveString, colour);
        if (moveString == KINGSIDE_CASTLE) return parseKingsideCastle(moveString, colour);

        // If this is a promotion, handle that
        if (moveString.Contains(PROMOTION)) return parsePromotion(moveString, colour);

        // First, find the piece's name based off the move string
        var (pieceName, coordinateString) = parseName(moveString);

        // Then convert the coordinates to our format (fromRow, fromColumn, toRow, toColumn)
        // Noting that startRow and startColumn will usually be -1 indicating they're not present.
        // They will only be used if necessary to disambiguate
        var (fromRow, fromColumn, toRow, toColumn) = parseCoordinates(coordinateString);

        // Then look through ValidMoves[Colour] for moves with matching toRow, toColumn
        var moveCandidates = Board.ValidMoves[colour].FindAll(m => m.ToRow == toRow && m.ToColumn == toColumn);

        // If moves.Count == 1, return that move
        if (moveCandidates.Count == 1) return moveCandidates[0];

        // Disambiguate the move.
        foreach (var candidate in moveCandidates) {
            // If we got a startRow or startColumn provided in the moveString, use that.
            var (row, column) = (candidate.FromRow, candidate.FromColumn);
            var piece = Board.Pieces[row, column];
            var name = piece.Name;

            // Try most specific first.
            if (fromRow == row && fromColumn == column && pieceName == name) return candidate;
            if (fromRow == row && pieceName == name) return candidate;
            if (fromColumn == column && pieceName == name) return candidate;

            // If there is no fromRow or fromColumn, then we're guaranteed the piece name will be unique.
            if (fromRow == -1 && fromColumn == -1) {
                if (name == pieceName) return candidate;
            }
        }

        throw new System.Exception($"No move found matching {moveString}");
    }

    private Move parsePromotion(string moveString, ChessPiece.EColour colour) {
        var split = moveString.Split('=');
        var coordinates = split[0];
        var p = split[1];
        var (pieceName, _) = parseName($"{p} "); // needs to be done to trick parseName

        var (fromRow, fromColumn, toRow, toColumn) = parseCoordinates(coordinates);
        var promotionMove = Board.ValidMoves[colour].Find(m => {
            if (fromColumn == -1) return m.PieceToPromoteTo == pieceName && m.ToColumn == toColumn;
            else return m.PieceToPromoteTo == pieceName && m.ToColumn == toColumn && m.FromColumn == fromColumn;
        });

        if (promotionMove == null) throw new System.Exception($"Unable to find promotion move for {moveString}");

        return promotionMove;
    }

    private Move parseKingsideCastle(string moveString, ChessPiece.EColour colour) {
        var row = colour == ChessPiece.EColour.Black ? 0 : 7;
        return Board.ValidMoves[colour].Find(m => m.FromRow == row && m.FromColumn == 4 && m.ToColumn == 6 && m.ToRow == row);

    }

    private Move parseQueensideCastle(string moveString, ChessPiece.EColour colour) {
        var row = colour == ChessPiece.EColour.Black ? 0 : 7;
        return Board.ValidMoves[colour].Find(m => m.FromRow == row && m.FromColumn == 4 && m.ToColumn == 2 && m.ToRow == row);
    }

    private (int, int, int, int) parseCoordinates(string coordinateString) {
        int fromRow = -1, fromColumn = -1, toRow, toColumn;

        // Move with no origin rank OR file
        if (coordinateString.Length == 2) {
            var rank = coordinateString[1]; // row
            var file = coordinateString[0]; // column
            toRow = RANK_TO_ROW[rank];
            toColumn = FILE_TO_COLUMN[file];
            return (fromRow, fromColumn, toRow, toColumn);
        }

        // Move specifying origin rank OR file
        if (coordinateString.Length == 3) {
            char rank, file;
            var firstChar = coordinateString[0];
            if (RANK_TO_ROW.ContainsKey(firstChar)) fromRow = RANK_TO_ROW[firstChar];
            if (FILE_TO_COLUMN.ContainsKey(firstChar)) fromColumn = FILE_TO_COLUMN[firstChar];

            rank = coordinateString[2]; // row
            file = coordinateString[1]; // column
            toRow = RANK_TO_ROW[rank];
            toColumn = FILE_TO_COLUMN[file];
            return (fromRow, fromColumn, toRow, toColumn);
        }

        // Move specifying origin rank and file
        if (coordinateString.Length == 4) {
            var fromRank = coordinateString[1]; // row
            var fromFile = coordinateString[0]; // column
            fromRow = RANK_TO_ROW[fromRank];
            fromColumn = FILE_TO_COLUMN[fromFile];

            var rank = coordinateString[3]; // row
            var file = coordinateString[2]; // column
            toRow = RANK_TO_ROW[rank];
            toColumn = FILE_TO_COLUMN[file];

            return (fromRow, fromColumn, toRow, toColumn);
        }

        throw new System.Exception($"Invalid coordinates: {coordinateString}");
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

        throw new System.Exception($"Unable to parse invalid name string: {moveString}");
    }
}