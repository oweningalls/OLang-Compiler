using Lexing;
using OLangGrammar.ParseTree.Type;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.ClassMember;

public class FieldDeclaration(IType type, IdentifierToken identifier) : IClassMember
{
    public IType Type = type;
    public IdentifierToken Identifier = identifier;
    public SourceSpan Span { get; set; }
}