using OLangCompiler.Parser.ParseTree.ParameterList;
using OLangCompiler.Parser.ParseTree.Scope;
using OLangTokens.Tokens;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class VoidFunctionDeclaration(IdentifierToken identifier, IParameterListNode parameters, IScopeNode scope) : IStatement
{
    public IdentifierToken Identifier = identifier;
    public IParameterListNode Parameters = parameters;
    public IScopeNode Scope = scope;
}