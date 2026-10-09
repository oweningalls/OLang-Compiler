using Lexing;
using OLangGrammar.ParseTree.Scope;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.ClassMember;

public class VoidMethodDeclaration(IdentifierToken identifier, IScopeNode scope, bool isInstance) : IClassMember
{
    public IdentifierToken Identifier = identifier;
    public IScopeNode Scope = scope;
    public bool IsInstance = isInstance;
    public SourceSpan Span { get; set; }
}