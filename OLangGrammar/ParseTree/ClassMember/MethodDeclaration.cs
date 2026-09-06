using Lexing;
using OLangGrammar.ParseTree.Scope;
using OLangGrammar.ParseTree.Type;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.ClassMember;

public class MethodDeclaration(IType type, IdentifierToken identifier, IScopeNode scope) : IClassMember
{
    public IType Type = type;
    public IdentifierToken Identifier = identifier;
    public IScopeNode Scope = scope;
    public SourceSpan Span { get; set; }
}