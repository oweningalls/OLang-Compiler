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
        Lr0ParseTable? table = null;
        try
        {
            table = new Lr0ParseTable(new NonLr0Grammar(), new ErrorHelper(""));
            Assert.Fail("Parse table should throw exception for non-LR(0) grammar");
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    [Test]
    public void TestAmbiguousFails()
    {
        Lr0ParseTable? table = null;
        try
        {
            table = new Lr0ParseTable(new AmbiguousGrammar(), new ErrorHelper(""));
            Assert.Fail("Parse table should throw exception for ambiguous grammar");
        }
        catch (Exception)
        {
            // ignored
        }
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
        Lr0ParseTable? table = null;
        try
        {
            table = new Lr0ParseTable(new OLangGrammar(), new ErrorHelper(""));
            Assert.Fail("OLang is presumably not LR(0), so this should have conflicts");
        }
        catch (Exception)
        {
            // ignored
        }
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

    private interface ITestNode : INode;

    public class ANode : ITestNode;

    private class BNode : ITestNode;

    private class NonLr0Grammar : IGrammar
    {
        private List<BaseGrammarRule> _rules =
        [
            GrammarRule.Create((BNode _, IntLiteralToken _) => new ANode()),
            GrammarRule.Create((ANode _, IntLiteralToken _) => new ANode()),
            GrammarRule.Create((BoolLiteralToken _, IntLiteralToken _) => new ANode()),
            GrammarRule.Create((BoolLiteralToken _) => new BNode()),
        ];
        public Type GetStartSymbol()
        {
            return typeof(ANode);
        }

        public List<BaseGrammarRule> GetRules()
        {
            return _rules;
        }
    }

    private interface IANode : ITestNode;
    private class FloatANode(BoolLiteralToken boolLiteralToken, FloatLiteralToken floatLiteralToken) : IANode
    {
        public BoolLiteralToken BoolLiteralToken = boolLiteralToken;
        public FloatLiteralToken FloatLiteralToken = floatLiteralToken;
    }
    
    private class IntANode(BoolLiteralToken boolLiteralToken, IntLiteralToken intLiteralToken) : IANode
    {
        public BoolLiteralToken BoolLiteralToken = boolLiteralToken;
        public IntLiteralToken IntLiteralToken = intLiteralToken;
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
            GrammarRule.Create((BNode _) => new ANode()),
            GrammarRule.Create((ANode _) => new BNode()),
            GrammarRule.Create((IntLiteralToken _) => new ANode()),
        ];

        public Type GetStartSymbol()
        {
            return typeof(ANode);
        }

        public List<BaseGrammarRule> GetRules()
        {
            return _rules;
        }
    }
}