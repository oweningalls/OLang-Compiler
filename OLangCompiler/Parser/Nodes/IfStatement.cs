namespace OLangCompiler.Parser.Nodes;

public class IfStatement(IExpressionNode condition, ScopeNode scope, ElseNode? elseBlock) : IStatementNode
{
    public IExpressionNode Condition = condition;
    public ScopeNode Scope = scope;
    public ElseNode? ElseBlock = elseBlock;
}