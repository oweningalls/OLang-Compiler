using OLangCompiler.Parser.ParseTree.EqualityBinaryExpression;

namespace OLangCompiler.Parser.ParseTree.AndBinaryExpression;

public class NonAnd(IEqualityExpression expression) : IAndExpression
{
    public IEqualityExpression Expression = expression;
}