using Lexing;
using OLangGrammar.ParseTree.ArgumentList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class ClassInstantiationTerm(IdentifierToken identifier, IArgumentList argumentList) : ITerm
{
    public IdentifierToken Identifier = identifier;
    public IArgumentList ArgumentList = argumentList;
    public SourceSpan Span { get; set; }
}