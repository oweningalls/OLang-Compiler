using Lexing;
using OLangCompiler.Parser.BottomUpParser;
using OLangTokens.Tokens;

namespace OLangTests;

public class GrammarHelperTests
{
    [TestCase(typeof(IANode), 3)]
    [TestCase(typeof(IBNode), 1)]
    public void TestGetProductionsFor(Type type, int numProductions)
    {
        var helper = new GrammarHelper(new TestGrammar());
        
        Assert.That(helper.GetProductionsFor(type), Has.Count.EqualTo(numProductions));
    }

    [Test]
    public void TestEpsilonWorksWithFirst()
    {
        var helper = new GrammarHelper(new GrammarWithEpsilons());

        var first = helper.First(typeof(IANode));
        Assert.That(first.Count, Is.EqualTo(2));
        Assert.That(first, Does.Contain(typeof(BoolLiteralToken)));
        Assert.That(first, Does.Contain(typeof(IntLiteralToken)));
    }

    private class GrammarWithEpsilons : IGrammar
    {
        private List<BaseGrammarRule> _rules =
        [
            GrammarRule.Create((IBNode _, BoolLiteralToken _) => new ANode()),
            GrammarRule.Create((IntLiteralToken _) => new BNode()),
            GrammarRule.Create(() => new BNode()),
            
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

    [Test]
    public void TestANodeFirst()
    {
        var helper = new GrammarHelper(new TestGrammar());

        var aFirst = helper.First(typeof(IANode));
        
        Assert.That(aFirst, Has.Count.EqualTo(2));
        Assert.That(aFirst, Does.Contain(typeof(BoolLiteralToken)));
        Assert.That(aFirst, Does.Contain(typeof(IntLiteralToken)));
    }
    
    [Test]
    public void TestBNodeFirst()
    {
        var helper = new GrammarHelper(new TestGrammar());

        var aFirst = helper.First(typeof(IBNode));
        
        Assert.That(aFirst, Has.Count.EqualTo(1));
        Assert.That(aFirst, Does.Contain(typeof(BoolLiteralToken)));
    }
}