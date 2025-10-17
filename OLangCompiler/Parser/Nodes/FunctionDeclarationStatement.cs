using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.Nodes;

public class FunctionDeclarationStatement(ExpressionType? type, IdentifierToken identifier, ParameterListNode parameters, ScopeNode scope) : IStatementNode
{
    public ExpressionType? Type = type;
    public IdentifierToken Identifier = identifier;
    public ParameterListNode Parameters = parameters;
    public ScopeNode Scope = scope;
}