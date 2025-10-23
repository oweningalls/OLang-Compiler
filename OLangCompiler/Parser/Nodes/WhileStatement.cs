namespace OLangCompiler.Parser.Nodes;

public class WhileStatement(BaseExpressionNode condition, ScopeNode scope) : IStatementNode
{
    public BaseExpressionNode Condition = condition;
    public ScopeNode Scope = scope;
}