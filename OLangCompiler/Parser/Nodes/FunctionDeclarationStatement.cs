using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.Nodes;

public class FunctionDeclarationStatement(IFunctionType type, IdentifierToken identifier, IParameterListNode parameters, ScopeNode scope) : IStatementNode
{
    public IFunctionType Type = type;
    public IdentifierToken Identifier = identifier;
    public IParameterListNode Parameters = parameters;
    public ScopeNode Scope = scope;
}