using Lexing;
using OLangGrammar.ParseTree.ParameterList;
using OLangGrammar.ParseTree.Scope;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Stmt;

public class VoidFunctionDeclarationWithParameters(IdentifierToken identifier, IParameterListNode parameters, IScopeNode scope) : IStatement
{
    public IdentifierToken Identifier = identifier;
    public IParameterListNode Parameters = parameters;
    public IScopeNode Scope = scope;
    public SourceSpan Span { get; set; }
}