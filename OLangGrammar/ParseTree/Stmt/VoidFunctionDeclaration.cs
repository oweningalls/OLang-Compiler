using Lexing;
using OLangGrammar.ParseTree.Scope;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Stmt;

public class VoidFunctionDeclaration(IdentifierToken identifier, IScopeNode scope) : IStatement
{
    public IdentifierToken Identifier = identifier;
    public IScopeNode Scope = scope;
    public SourceSpan Span { get; set; }
}