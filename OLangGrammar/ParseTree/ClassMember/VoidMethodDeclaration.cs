using Lexing;
using OLangGrammar.ParseTree.Scope;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.ClassMember;

public class VoidMethodDeclaration(IdentifierToken identifier, IScopeNode scope) : IClassMember
{
    public IdentifierToken Identifier = identifier;
    public IScopeNode Scope = scope;
    public SourceSpan Span { get; set; }
}