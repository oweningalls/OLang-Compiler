using OLangGrammar.ParseTree.GreaterExpression;

namespace OLangGrammar.ParseTree.EqualityExpression;

public class NonEquality(IGreaterExpression expression) : IEqualityExpression
{
    public IGreaterExpression Expression = expression;
}