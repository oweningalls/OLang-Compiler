using Lexing;
using OLangGrammar.ParseTree.TypeArgumentList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.EnumInstantiation;

public class GenericEnumVariantInstantiation(IdentifierToken enumName, ITypeArgumentList typeArgumentList, IdentifierToken variantName) : IEnumInstantiation
{
    public IdentifierToken EnumName = enumName;
    public ITypeArgumentList TypeArgumentList = typeArgumentList;
    public IdentifierToken VariantName = variantName;
    public SourceSpan Span { get; set; }
}