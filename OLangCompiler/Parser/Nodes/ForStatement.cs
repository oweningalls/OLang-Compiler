using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class ForStatement(IdentifierToken identifier, BaseExpressionNode start, BaseExpressionNode end, ScopeNode scope) : IStatementNode
{
    public IdentifierToken Identifier = identifier;
    public BaseExpressionNode Start = start;
    public BaseExpressionNode End = end;
    public ScopeNode Scope = scope;
}