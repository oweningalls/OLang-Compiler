using OLangCompiler.Parser.ParseTree.AddBinaryExpression;

namespace OLangCompiler.Parser.ParseTree.GreaterBinaryExpression;

public class NonGreater(IAddExpression expression) : IGreaterExpression
{
    public IAddExpression Expression = expression;
}