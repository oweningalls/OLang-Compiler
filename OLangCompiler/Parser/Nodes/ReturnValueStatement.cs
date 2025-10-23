namespace OLangCompiler.Parser.Nodes;

public class ReturnValueStatement(BaseExpressionNode expression) : IStatementNode
{
    public BaseExpressionNode Expression = expression;
}