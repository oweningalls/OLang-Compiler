using Lexing;
using OLangGrammar.ParseTree.ArgumentList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class EnumVariantInstantiationWithArguments(IdentifierToken enumName, IdentifierToken variantName, IArgumentList argumentList) : ITerm
{
    public IdentifierToken EnumName = enumName;
    public IdentifierToken VariantName = variantName;
    public IArgumentList ArgumentList = argumentList;
    public SourceSpan Span { get; set; }
}