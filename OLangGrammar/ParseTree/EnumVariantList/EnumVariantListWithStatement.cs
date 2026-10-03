using Lexing;
using OLangGrammar.ParseTree.EnumVariant;

namespace OLangGrammar.ParseTree.EnumVariantList;

public class EnumVariantListWithStatement(IEnumVariant enumVariant, IEnumVariantList enumVariantList) : IEnumVariantList
{
    public IEnumVariant EnumVariant = enumVariant;
    public IEnumVariantList EnumVariantList = enumVariantList;
    public SourceSpan Span { get; set; }
}