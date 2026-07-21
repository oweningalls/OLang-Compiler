using OLangCompiler;
using OLangCompiler.Parser.BottomUpParser;
using OLangCompiler.Parser.BottomUpParser.Lr1;
using OLangCompiler.Tokens;

namespace OLangTests;

public class Lr1ParserTests
{
    [Test]
    public void TestGrammarDoesntFail()
    {
        Assert.DoesNotThrow(() => new Lr1ParseTable(new TestGrammar(), new ErrorHelper("")));
    }
    
    [Test]
    public void TestAmbiguousFails()
    {
        var e = Assert.Throws<Exception>(() => _ = new Lr1ParseTable(new AmbiguousGrammar(), new ErrorHelper("")));
        Assert.That(e.Message.Contains("conflict", StringComparison.CurrentCultureIgnoreCase), e.Message);
    }

    [Test]
    public void TestLr0GrammarWorks()
    {
        Lr1ParseTable? table = null;
        Assert.DoesNotThrow(() => table = new Lr1ParseTable(new Lr0Grammar(), new ErrorHelper("")));
    }

    [Test]
    public void TestOLangGrammar()
    {
        Assert.DoesNotThrow(() => new Lr1ParseTable(new OLangGrammar(), new ErrorHelper("")));
    }

    // [Test]
    // public void TestParseLr0Grammar()
    // {
    //     var parser = new Lr1Parser();
    //     var input = new List<BaseToken>
    //     {
    //         new BoolLiteralToken(false), new IntLiteralToken(1)
    //     };
    //     
    //     var program = parser.ParseProgram(new Lr0Grammar(), input, new ErrorHelper(""));
    //     var intNode = program as IntANode;
    //     Assert.That(intNode, Is.Not.Null);
    //     Assert.That(intNode.BoolLiteralToken.Value, Is.False);
    //     Assert.That(intNode.IntLiteralToken.Value, Is.EqualTo(1));
    // }
}