using OLangCompiler.Parser.ParseTree.MultBinaryExpression;

namespace OLangCompiler.Parser.ParseTree.AddBinaryExpression;

public class Plus(IAddExpression lhs, IMultExpression rhs) : IAddExpression
{
    public IAddExpression Lhs = lhs;
    public IMultExpression Rhs = rhs;
}
