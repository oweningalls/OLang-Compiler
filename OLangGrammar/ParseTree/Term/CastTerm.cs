using Lexing;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.Type;

namespace OLangGrammar.ParseTree.Term;

public class CastTerm(IType type, IExpression expression) : ITerm
{
    public IType Type = type;
    public IExpression Expression = expression;
    public SourceSpan Span { get; set; }
}