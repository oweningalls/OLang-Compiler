namespace OLangCompiler.Parser.Nodes;

public class ParenTerm(IExpressionNode expression) : ITermNode
{
    public IExpressionNode Expression = expression;
}