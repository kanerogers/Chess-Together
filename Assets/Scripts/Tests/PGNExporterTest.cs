using NUnit.Framework;
using System.IO;

namespace UnitTests {
    public class PGNExporterTest {
        [Test]
        public void BasicPGNTest() {
            var board = new ChessBoard();

            var move = new Move(6, 0, 5, 0);
            Assert.IsTrue(board.Move(move));
            move = new Move(0, 1, 2, 2);
            Assert.IsTrue(board.Move(move));

            var pgn = PGNExporter.ToPGN(board);
            var expected = "1.a3 Nc6";
            Assert.AreEqual(expected, pgn);
        }

        [Test]
        public void FullPGNTest() {
            var exportPath = "Assets\\Scripts\\Tests\\export.pgn";
            var importPath = "Assets\\Scripts\\Tests\\one.pgn";
            var parser = new PGNParser(importPath);
            parser.Parse(1);
            var board = parser.Board;
            var pgn = PGNExporter.ToPGN(board);

            var expected = File.ReadAllText(exportPath);
            Assert.AreEqual(expected, pgn);
        }

    }
}