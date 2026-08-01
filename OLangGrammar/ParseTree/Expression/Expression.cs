using OLangGrammar.ParseTree.AndExpression;

namespace OLangGrammar.ParseTree.Expression;

public class Expression(IExpression lhs, IAndExpression rhs) : IExpression
{
    public IExpression Lhs = lhs;
    public IAndExpression Rhs = rhs;
}