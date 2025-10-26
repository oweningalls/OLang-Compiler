using OLangCompiler.Parser.ParseTree.EqualityBinaryExpression;

namespace OLangCompiler.Parser.ParseTree.AndBinaryExpression;

public class AndExpression(IAndExpression lhs, IEqualityExpression rhs) : IAndExpression
{
    public IAndExpression Lhs = lhs;
    public IEqualityExpression Rhs = rhs;
}
