using OLangCompiler.Parser.ParseTree.EqualityExpression;

namespace OLangCompiler.Parser.ParseTree.AndExpression;

public class NonAnd(IEqualityExpression expression) : IAndExpression
{
    public IEqualityExpression Expression = expression;
}