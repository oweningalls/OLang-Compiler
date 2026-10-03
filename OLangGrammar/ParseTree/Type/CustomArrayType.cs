using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Type;

public class CustomArrayType(IdentifierToken innerType) : IType
{
    public IdentifierToken InnerType = innerType;
    public SourceSpan Span { get; set; }
}