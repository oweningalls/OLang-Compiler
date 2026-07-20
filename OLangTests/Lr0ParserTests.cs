using OLangCompiler;
using OLangCompiler.Parser.BottomUpParser;
using OLangCompiler.Parser.BottomUpParser.Lr0;
using OLangCompiler.Parser.ParseTree;
using OLangCompiler.Tokens;

namespace OLangTests;

public class Lr0ParserTests
{
    [Test]
    public void TestNonLr0Fails()
    {
        var e = Assert.Throws<Exception>(() => _ = new Lr0ParseTable(new TestGrammar(), new ErrorHelper("")));
        Assert.That(e.Message.Contains("reduce/reduce conflict", StringComparison.CurrentCultureIgnoreCase) || e.Message.Contains("shift/reduce conflict", StringComparison.CurrentCultureIgnoreCase));
    }
    
    [Test]
    public void TestAmbiguousFails()
    {
        var e = Assert.Throws<Exception>(() => _ = new Lr0ParseTable(new AmbiguousGrammar(), new ErrorHelper("")));
        Assert.That(e.Message.Contains("reduce/reduce conflict", StringComparison.CurrentCultureIgnoreCase));
    }

    [Test]
    public void TestLr0GrammarWorks()
    {
        Lr0ParseTable? table = null;
        Assert.DoesNotThrow(() => table = new Lr0ParseTable(new Lr0Grammar(), new ErrorHelper("")));
    }

    [Test]
    public void TestOLangGrammar()
    {
        var e = Assert.Throws<Exception>(() => new Lr0ParseTable(new OLangGrammar(), new ErrorHelper("")));
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
        
        var program = parser.ParseProgram(new Lr0Grammar(), input, new ErrorHelper(""));
        var intNode = program as IntANode;
        Assert.That(intNode, Is.Not.Null);
        Assert.That(intNode.BoolLiteralToken.Value, Is.False);
        Assert.That(intNode.IntLiteralToken.Value, Is.EqualTo(1));
    }

    private class Lr0Grammar : IGrammar
    {
        private List<BaseGrammarRule> _rules =
        [
            GrammarRule.Create((BoolLiteralToken b, IntLiteralToken i) => new IntANode(b, i)),
            GrammarRule.Create((BoolLiteralToken b, FloatLiteralToken f) => new FloatANode(b, f)),
        ];
        public Type GetStartSymbol()
        {
            return typeof(IANode);
        }

        public List<BaseGrammarRule> GetRules()
        {
            return _rules;
        }
    }

    private class AmbiguousGrammar : IGrammar
    {
        private List<BaseGrammarRule> _rules =
        [
            GrammarRule.Create((IBNode _) => new ANode()),
            GrammarRule.Create((IANode _) => new BNode()),
            GrammarRule.Create((IBNode _) => new BNode()),
        ];

        public Type GetStartSymbol()
        {
            return typeof(IANode);
        }

        public List<BaseGrammarRule> GetRules()
        {
            return _rules;
        }
    }
}