using Lexing;
using OLangGrammar.ParseTree.EnumVariant;

namespace OLangGrammar.ParseTree.EnumVariantList;

public class SingleVariantEnumVariantList(IEnumVariant enumVariant) : IEnumVariantList
{
    public IEnumVariant EnumVariant = enumVariant;
    public SourceSpan Span { get; set; }
}