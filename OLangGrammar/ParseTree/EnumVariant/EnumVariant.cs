using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.EnumVariant;

public class EnumVariant(IdentifierToken identifier) : IEnumVariant
{
    public IdentifierToken Identifier = identifier;

    public SourceSpan Span { get; set; }
}