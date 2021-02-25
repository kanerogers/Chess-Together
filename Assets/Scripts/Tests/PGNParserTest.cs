using NUnit.Framework;
public class PGNParserTest {
    [Test]
    public void BasicReadPGNFile() {
        var parser = new PGNParser("Assets\\Scripts\\Tests\\test.pgn");
        var moves = parser.GetMoves();
        Assert.AreEqual(moves.Count, 16);

        var firstMove = moves[0];
        var expectedMove = new Move(6, 4, 4, 4);
        Assert.AreEqual(firstMove, expectedMove);
    }
}