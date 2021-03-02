using NUnit.Framework;
namespace Tests {
    public class PGNParserTest {
        [Test]
        public void BasicReadPGNFile() {
            var parser = new PGNParser("Assets\\Scripts\\Tests\\test.pgn");
            var moves = parser.GetMoves();
            Assert.AreEqual(moves.Count, 63);

            var move = moves[0];
            var expectedMove = new Move(6, 4, 4, 4);
            Assert.AreEqual(move, expectedMove);

            move = moves[1];
            expectedMove = new Move(1, 4, 2, 4);
            Assert.AreEqual(move, expectedMove);
        }
    }
}