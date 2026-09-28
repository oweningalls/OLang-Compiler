using Lexing;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.Type;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class ArrayInstantiationTerm(IType type, IExpression size) : ITerm
{
    public IType Type = type;
    public IExpression Size = size;
    public SourceSpan Span { get; set; }
}