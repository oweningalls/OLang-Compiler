using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Type;

public class NonPrimitiveType(IdentifierToken identifier) : IType
{
    public IdentifierToken Identifier = identifier;
    public SourceSpan Span { get; set; }
}