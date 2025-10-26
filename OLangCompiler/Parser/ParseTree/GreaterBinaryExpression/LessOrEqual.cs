using OLangCompiler.Parser.ParseTree.AddBinaryExpression;

namespace OLangCompiler.Parser.ParseTree.GreaterBinaryExpression;

public class LessOrEqual(IGreaterExpression lhs, IAddExpression rhs) : IGreaterExpression
{
    public IGreaterExpression Lhs = lhs;
    public IAddExpression Rhs = rhs;
}
