using Lexing;
using OLangGrammar.ParseTree.Expression;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class CustomTypeArrayInstantiationTerm(IdentifierToken type, IExpression size) : ITerm
{
    public IdentifierToken Type = type;
    public IExpression Size = size;
    public SourceSpan Span { get; set; }
}