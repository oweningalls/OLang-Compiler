using OLangAst;
using OLangGrammar.ParseTree.Term;
using OLangTokens.Tokens;

namespace OLangTests;

public class AstConversionTests() : OLangAstBuilder(new NoOpErrorHelper())
{
    [Test]
    public void TestParseTerm_IntLit()
    {
        var value = 11;
        var parsed = new IntLiteral(new IntLiteralToken(value));

        var converted = ParseTerm(parsed);
        
        Assert.That(parsed, Is.EqualTo(converted));
    }
}