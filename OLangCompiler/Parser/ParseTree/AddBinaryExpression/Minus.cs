using OLangCompiler.Parser.ParseTree.MultBinaryExpression;

namespace OLangCompiler.Parser.ParseTree.AddBinaryExpression;

public class Minus(IAddExpression lhs, IMultExpression rhs) : IAddExpression
{
    public IAddExpression Lhs = lhs;
    public IMultExpression Rhs = rhs;
}
