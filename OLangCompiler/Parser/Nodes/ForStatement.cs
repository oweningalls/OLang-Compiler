using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class ForStatement(IdentifierToken identifier, IExpressionNode start, IExpressionNode end, ScopeNode scope) : IStatementNode
{
    public IdentifierToken Identifier = identifier;
    public IExpressionNode Start = start;
    public IExpressionNode End = end;
    public ScopeNode Scope = scope;
}