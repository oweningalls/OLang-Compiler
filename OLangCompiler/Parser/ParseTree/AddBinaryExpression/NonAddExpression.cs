using OLangCompiler.Parser.ParseTree.MultBinaryExpression;

namespace OLangCompiler.Parser.ParseTree.AddBinaryExpression;

public class NonAddExpression(IMultExpression expression) : IAddExpression
{
    public IMultExpression Expression = expression;
}
