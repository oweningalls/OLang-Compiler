using OLangGrammar.ParseTree.AddExpression;

namespace OLangGrammar.ParseTree.GreaterExpression;

public class Less(IGreaterExpression lhs, IAddExpression rhs) : IGreaterExpression
{
    public IGreaterExpression Lhs = lhs;
    public IAddExpression Rhs = rhs;
}
