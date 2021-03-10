using NUnit.Framework;
namespace Tests {
    public class PGNParserTest {
        [Test]
        public void ReadOneGame() {
            var parser = new PGNParser("Assets\\Scripts\\Tests\\test.pgn");
            var games = parser.Parse(1);
            Assert.AreEqual(1, games.Count);
            var moves = games[0];
            Assert.AreEqual(85, moves.Count);
        }

        [Test]
        public void ReadAll() {
            var parser = new PGNParser("Assets\\Scripts\\Tests\\test.pgn");
            var games = parser.Parse();
            Assert.AreEqual(3251, games.Count);
        }
    }
}