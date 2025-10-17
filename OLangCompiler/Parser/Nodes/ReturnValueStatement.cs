namespace OLangCompiler.Parser.Nodes;

public class ReturnValueStatement(IExpressionNode expression) : IStatementNode
{
    public IExpressionNode Expression = expression;
}