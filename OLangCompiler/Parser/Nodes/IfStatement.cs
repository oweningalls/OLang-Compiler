namespace OLangCompiler.Parser.Nodes;

public class IfStatement(IExpressionNode condition, ScopeNode scope) : IStatementNode
{
    public IExpressionNode Condition = condition;
    public ScopeNode Scope = scope;
}