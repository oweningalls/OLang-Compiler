using OLangCompiler.Parser.ParseTree.AndExpression;

namespace OLangCompiler.Parser.ParseTree.Expression;

public class NonExpression(IAndExpression expression) : IExpression
{
    public IAndExpression Expression = expression;
}