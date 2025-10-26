using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.Scope;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class ForStatement(IdentifierToken identifier, BaseExpressionNode start, BaseExpressionNode end, ScopeNode scope) : IStatementNode
{
    public IdentifierToken Identifier = identifier;
    public BaseExpressionNode Start = start;
    public BaseExpressionNode End = end;
    public ScopeNode Scope = scope;
}