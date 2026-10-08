using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.EnumInstantiation;

public class EnumVariantInstantiation(IdentifierToken enumName, IdentifierToken variantName) : IEnumInstantiation
{
    public IdentifierToken EnumName = enumName;
    public IdentifierToken VariantName = variantName;
    public SourceSpan Span { get; set; }
}