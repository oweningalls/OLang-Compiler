using OLangCompiler.Parser.ParseTree.GreaterBinaryExpression;

namespace OLangCompiler.Parser.ParseTree.EqualityBinaryExpression;

public class NonEquality(IGreaterExpression expression) : IEqualityExpression
{
    public IGreaterExpression Expression = expression;
}