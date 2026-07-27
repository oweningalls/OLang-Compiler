using OLangCompiler.Parser.BottomUpParser;
using OLangCompiler.Parser.BottomUpParser.Lr0;
using OLangCompiler.Tokens;

namespace OLangTests;

public class Lr0ParserTests
{
    [Test]
    public void TestNonLr0Fails()
    {
        var e = Assert.Throws<Exception>(() => _ = new Lr0ParseTable(new TestGrammar(), new NoOpErrorHelper()));
        Assert.That(e.Message.Contains("reduce/reduce conflict", StringComparison.CurrentCultureIgnoreCase) || e.Message.Contains("shift/reduce conflict", StringComparison.CurrentCultureIgnoreCase));
    }
    
    [Test]
    public void TestAmbiguousFails()
    {
        var e = Assert.Throws<Exception>(() => _ = new Lr0ParseTable(new AmbiguousGrammar(), new NoOpErrorHelper()));
        Assert.That(e.Message.Contains("reduce/reduce conflict", StringComparison.CurrentCultureIgnoreCase));
    }

    [Test]
    public void TestLr0GrammarWorks()
    {
        Lr0ParseTable? table = null;
        Assert.DoesNotThrow(() => table = new Lr0ParseTable(new Lr0Grammar(), new NoOpErrorHelper()));
    }

    [Test]
    public void TestOLangGrammar()
    {
        var e = Assert.Throws<Exception>(() => new Lr0ParseTable(new OLangGrammar(), new NoOpErrorHelper()));
        Assert.That(e.Message.Contains("reduce/reduce conflict", StringComparison.CurrentCultureIgnoreCase) || e.Message.Contains("shift/reduce conflict", StringComparison.CurrentCultureIgnoreCase));
    }

    [Test]
    public void TestParseLr0Grammar()
    {
        var parser = new Lr0Parser();
        var input = new List<BaseToken>
        {
            new BoolLiteralToken(false), new IntLiteralToken(1)
        };
        
        var program = parser.ParseProgram(new Lr0Grammar(), input, new NoOpErrorHelper());
        var intNode = program as IntANode;
        Assert.That(intNode, Is.Not.Null);
        Assert.That(intNode.BoolLiteralToken.Value, Is.False);
        Assert.That(intNode.IntLiteralToken.Value, Is.EqualTo(1));
    }
}