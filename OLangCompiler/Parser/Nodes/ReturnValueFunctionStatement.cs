namespace OLangCompiler.Parser.Nodes;

public class ReturnValueFunctionStatement(IExpressionNode expression) : IFunctionStatementNode
{
    public IExpressionNode Expression = expression;
}