namespace OLangCompiler.Parser.Nodes;

public class IfStatement(BaseExpressionNode condition, ScopeNode scope, ElseNode? elseBlock) : IStatementNode
{
    public BaseExpressionNode Condition = condition;
    public ScopeNode Scope = scope;
    public ElseNode? ElseBlock = elseBlock;
}