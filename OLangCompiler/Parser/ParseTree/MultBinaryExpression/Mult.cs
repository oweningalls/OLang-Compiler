using OLangCompiler.Parser.ParseTree.UnaryExpression;

namespace OLangCompiler.Parser.ParseTree.MultBinaryExpression;

public class Mult(IMultExpression lhs, IUnaryExpression rhs) : IMultExpression
{
    public IMultExpression Lhs = lhs;
    public IUnaryExpression Rhs = rhs;
}
