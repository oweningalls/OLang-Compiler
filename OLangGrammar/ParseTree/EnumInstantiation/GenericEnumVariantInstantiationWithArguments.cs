using Lexing;
using OLangGrammar.ParseTree.ArgumentList;
using OLangGrammar.ParseTree.TypeArgumentList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.EnumInstantiation;

public class GenericEnumVariantInstantiationWithArguments(IdentifierToken enumName, ITypeArgumentList typeArgumentList, IdentifierToken variantName, IArgumentList argumentList) : IEnumInstantiation
{
    public IdentifierToken EnumName = enumName;
    public ITypeArgumentList TypeArgumentList = typeArgumentList;
    public IdentifierToken VariantName = variantName;
    public IArgumentList ArgumentList = argumentList;
    public SourceSpan Span { get; set; }
}