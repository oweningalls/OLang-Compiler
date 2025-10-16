using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.Nodes;

public class FunctionDeclarationStatement(ExpressionType? type, IdentifierToken identifier, FunctionScopeNode functionScope) : IStatementNode
{
    public ExpressionType? Type = type;
    public IdentifierToken Identifier = identifier;
    public FunctionScopeNode FunctionScope = functionScope;
}