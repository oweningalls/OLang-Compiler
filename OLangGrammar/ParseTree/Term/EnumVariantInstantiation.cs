using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class EnumVariantInstantiation(IdentifierToken enumName, IdentifierToken variantName) : ITerm
{
    public IdentifierToken EnumName = enumName;
    public IdentifierToken VariantName = variantName;
    public SourceSpan Span { get; set; }
}