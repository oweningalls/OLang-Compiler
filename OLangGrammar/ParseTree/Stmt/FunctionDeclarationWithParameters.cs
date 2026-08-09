using Lexing;
using OLangGrammar.ParseTree.ParameterList;
using OLangGrammar.ParseTree.Scope;
using OLangGrammar.ParseTree.Type;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Stmt;

public class FunctionDeclarationWithParameters(IType type, IdentifierToken identifier, IParameterListNode parameters, IScopeNode scope) : IStatement
{
    public IType Type = type;
    public IdentifierToken Identifier = identifier;
    public IParameterListNode Parameters = parameters;
    public IScopeNode Scope = scope;
    public SourceSpan Span { get; set; }
}