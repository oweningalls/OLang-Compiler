using OLangCompiler.Parser.BottomUpParser;
using OLangCompiler.Tokens;

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
    public void TestANodeFirst()
    {
        var helper = new GrammarHelper(new TestGrammar());

        var aFirst = helper.First(typeof(IANode));
        
        Assert.That(aFirst, Has.Count.EqualTo(1));
        Assert.That(aFirst, Does.Contain(typeof(BoolLiteralToken)));
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