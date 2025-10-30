using OLangCompiler.Parser.BottomUpParser;
using OLangCompiler.Parser.BottomUpParser.Lr0;
using OLangCompiler.Parser.ParseTree;
using OLangCompiler.Tokens;

namespace OLangTests;

public class Lr0ParseTableTests
{
    [Test]
    public void TestNonLr0Fails()
    {
        Lr0ParseTable? table = null;
        try
        {
            table = new Lr0ParseTable(new NonLr0Grammar());
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
            table = new Lr0ParseTable(new AmbiguousGrammar());
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
        Assert.DoesNotThrow(() => table = new Lr0ParseTable(new Lr0Grammar()));
    }
    
    private class ANode : INode;
    private class BNode : INode;
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

    private class Lr0Grammar : IGrammar
    {
        private List<BaseGrammarRule> _rules =
        [
            GrammarRule.Create((BoolLiteralToken _, IntLiteralToken _) => new ANode()),
            GrammarRule.Create((BoolLiteralToken _, FloatLiteralToken _) => new ANode()),
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