namespace OLangCompiler.Parser.ParseTree.Expression;

public class NotExpression(BaseExpression expression) : BaseExpression
{
    public BaseExpression Expression = expression;
}