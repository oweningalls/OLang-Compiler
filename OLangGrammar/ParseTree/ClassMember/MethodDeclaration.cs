using Lexing;
using OLangGrammar.ParseTree.Scope;
using OLangGrammar.ParseTree.Type;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.ClassMember;

public class MethodDeclaration(IType type, IdentifierToken identifier, IScopeNode scope, bool isInstance) : IClassMember
{
    public IType Type = type;
    public IdentifierToken Identifier = identifier;
    public IScopeNode Scope = scope;
    public bool IsInstance = isInstance;
    public SourceSpan Span { get; set; }
}