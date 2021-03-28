using NUnit.Framework;

namespace Tests {
    public class MoveTest {
        [Test]
        public void ToPGNTest() {
            var board = new ChessBoard();
            var move = new Move(6, 0, 5, 0);
            var pgn = move.ToPGN(board);
            var expected = "1.a3";
            Assert.AreEqual(expected, pgn);
        }

    }
}