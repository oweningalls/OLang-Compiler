using OLangCompiler.Parser.ParseTree.EqualityExpression;

namespace OLangCompiler.Parser.ParseTree.AndExpression;

public class AndExpression(IAndExpression lhs, IEqualityExpression rhs) : IAndExpression
{
    public IAndExpression Lhs = lhs;
    public IEqualityExpression Rhs = rhs;
}
