using Lexing;
using OLangGrammar.ParseTree.EnumValueList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.EnumVariant;

public class EnumVariantWithValues(IdentifierToken identifier, IEnumValueList valueList) : IEnumVariant
{
    public IdentifierToken Identifier = identifier;
    public IEnumValueList ValueList = valueList;
    public SourceSpan Span { get; set; }
}