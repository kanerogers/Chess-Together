using NUnit.Framework;
namespace Tests {
    public class PGNParserTest {
        [Test]
        public void BasicReadPGNFile() {
            var parser = new PGNParser("Assets\\Scripts\\Tests\\test.pgn");
            var moves = parser.GetMoves();
            Assert.AreEqual(moves.Count, 244);
        }
    }
}