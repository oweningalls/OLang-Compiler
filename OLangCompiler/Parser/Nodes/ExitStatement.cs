namespace OLangCompiler.Parser.Nodes;

public class ExitStatement(IExpressionNode expression) : IStatementNode
{
    public IExpressionNode ExpressionNode = expression;
}