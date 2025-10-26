using OLangCompiler.Parser.ParseTree.MultExpression;

namespace OLangCompiler.Parser.ParseTree.AddExpression;

public class NonAddExpression(IMultExpression expression) : IAddExpression
{
    public IMultExpression Expression = expression;
}
