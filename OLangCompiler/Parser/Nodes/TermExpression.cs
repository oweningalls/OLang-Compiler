namespace OLangCompiler.Parser.Nodes;

public class TermExpression(BaseTermNode term) : BaseExpressionNode
{
    public BaseTermNode Term = term;
}