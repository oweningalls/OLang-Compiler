namespace OLangCompiler.Parser.Nodes;

public class ExitStatement(BaseExpressionNode expression) : IStatementNode
{
    public BaseExpressionNode ExpressionNode = expression;
}