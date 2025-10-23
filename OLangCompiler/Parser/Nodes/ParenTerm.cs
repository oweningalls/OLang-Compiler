namespace OLangCompiler.Parser.Nodes;

public class ParenTerm(BaseExpressionNode expression) : BaseTermNode
{
    public BaseExpressionNode Expression = expression;
}