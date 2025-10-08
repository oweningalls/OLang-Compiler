namespace OLangCompiler.Parser.Nodes;

public class WhileStatement(IExpressionNode condition, ScopeNode scope) : IStatementNode
{
    public IExpressionNode Condition = condition;
    public ScopeNode Scope = scope;
}