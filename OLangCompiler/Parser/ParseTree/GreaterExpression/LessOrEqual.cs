using OLangCompiler.Parser.ParseTree.AddExpression;

namespace OLangCompiler.Parser.ParseTree.GreaterExpression;

public class LessOrEqual(IGreaterExpression lhs, IAddExpression rhs) : IGreaterExpression
{
    public IGreaterExpression Lhs = lhs;
    public IAddExpression Rhs = rhs;
}
