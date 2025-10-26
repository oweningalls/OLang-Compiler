using OLangCompiler.Parser.ParseTree.AddExpression;

namespace OLangCompiler.Parser.ParseTree.GreaterExpression;

public class NonGreaterExpression(IAddExpression expression) : IGreaterExpression
{
    public IAddExpression Expression = expression;
}