using OLangCompiler.Parser.ParseTree.AddExpression;

namespace OLangCompiler.Parser.ParseTree.GreaterExpression;

public class GreaterOrEqual(IGreaterExpression lhs, IAddExpression rhs) : IGreaterExpression
{
    public IGreaterExpression Lhs = lhs;
    public IAddExpression Rhs = rhs;
}
