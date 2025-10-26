using OLangCompiler.Parser.ParseTree.UnaryExpression;

namespace OLangCompiler.Parser.ParseTree.MultExpression;

public class NonMultExpression(IUnaryExpression expression) : IMultExpression
{
    public IUnaryExpression Expression = expression;
}