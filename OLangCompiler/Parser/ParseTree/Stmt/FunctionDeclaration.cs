using OLangCompiler.Parser.ParseTree.ParameterList;
using OLangCompiler.Parser.ParseTree.Scope;
using OLangCompiler.Parser.ParseTree.Type;
using OLangTokens.Tokens;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class FunctionDeclaration(IType type, IdentifierToken identifier, IParameterListNode parameters, IScopeNode scope) : IStatement
{
    public IType Type = type;
    public IdentifierToken Identifier = identifier;
    public IParameterListNode Parameters = parameters;
    public IScopeNode Scope = scope;
}