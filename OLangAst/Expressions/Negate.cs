using Lexing;

namespace OLangAst.Expressions;

public class Negate(IExpression value) : IExpression
{
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
}