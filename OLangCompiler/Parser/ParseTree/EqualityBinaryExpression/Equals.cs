using OLangCompiler.Parser.ParseTree.GreaterBinaryExpression;

namespace OLangCompiler.Parser.ParseTree.EqualityBinaryExpression;

public class Equals(IEqualityExpression lhs, IGreaterExpression rhs) : IEqualityExpression
{
    public IEqualityExpression Lhs = lhs;
    public IGreaterExpression Rhs = rhs;
}
