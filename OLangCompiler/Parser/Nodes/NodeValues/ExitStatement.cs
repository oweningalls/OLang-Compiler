namespace OLangCompiler.Parser.Nodes.NodeValues;

public class ExitStatement(IExpressionNode expression) : IStatementNode
{
    public IExpressionNode ExpressionNode = expression;
}