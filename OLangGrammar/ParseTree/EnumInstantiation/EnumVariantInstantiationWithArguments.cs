using Lexing;
using OLangGrammar.ParseTree.ArgumentList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.EnumInstantiation;

public class EnumVariantInstantiationWithArguments(IdentifierToken enumName, IdentifierToken variantName, IArgumentList argumentList) : IEnumInstantiation
{
    public IdentifierToken EnumName = enumName;
    public IdentifierToken VariantName = variantName;
    public IArgumentList ArgumentList = argumentList;
    public SourceSpan Span { get; set; }
}