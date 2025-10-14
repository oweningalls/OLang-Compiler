using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.Nodes;

public class FunctionDeclarationStatement(ExpressionType? type, IdentifierToken identifier, ScopeNode scope) : IStatementNode
{
    public ExpressionType? Type = type;
    public IdentifierToken Identifier = identifier;
    public ScopeNode Scope = scope;
}