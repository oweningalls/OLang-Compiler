using OLangCompiler.Parser.ParseTree.GreaterExpression;

namespace OLangCompiler.Parser.ParseTree.EqualityExpression;

public class DoubleEquals(IEqualityExpression lhs, IGreaterExpression rhs) : IEqualityExpression
{
    public IEqualityExpression Lhs = lhs;
    public IGreaterExpression Rhs = rhs;
}
