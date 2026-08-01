using OLangGrammar.ParseTree.EqualityExpression;

namespace OLangGrammar.ParseTree.AndExpression;

public class AndExpression(IAndExpression lhs, IEqualityExpression rhs) : IAndExpression
{
    public IAndExpression Lhs = lhs;
    public IEqualityExpression Rhs = rhs;
}
