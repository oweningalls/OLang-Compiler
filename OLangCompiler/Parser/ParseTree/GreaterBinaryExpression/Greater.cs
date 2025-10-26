using OLangCompiler.Parser.ParseTree.AddBinaryExpression;

namespace OLangCompiler.Parser.ParseTree.GreaterBinaryExpression;

public class Greater(IGreaterExpression lhs, IAddExpression rhs) : IGreaterExpression
{
    public IGreaterExpression Lhs = lhs;
    public IAddExpression Rhs = rhs;
}
