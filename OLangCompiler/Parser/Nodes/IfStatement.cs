namespace OLangCompiler.Parser.Nodes;

public class IfStatement(BaseExpressionNode condition, ScopeNode scope, IElseNode elseBlock) : IStatementNode
{
    public BaseExpressionNode Condition = condition;
    public ScopeNode Scope = scope;
    public IElseNode ElseBlock = elseBlock;
}