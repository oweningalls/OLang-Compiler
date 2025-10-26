using OLangCompiler.Parser.ParseTree.MultExpression;

namespace OLangCompiler.Parser.ParseTree.AddExpression;

public class Minus(IAddExpression lhs, IMultExpression rhs) : IAddExpression
{
    public IAddExpression Lhs = lhs;
    public IMultExpression Rhs = rhs;
}
