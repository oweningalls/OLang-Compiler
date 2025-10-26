using OLangCompiler.Parser.ParseTree.GreaterExpression;

namespace OLangCompiler.Parser.ParseTree.EqualityExpression;

public class NonEquality(IGreaterExpression expression) : IEqualityExpression
{
    public IGreaterExpression Expression = expression;
}