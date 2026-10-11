using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class Negate(IExpression value) : IExpression
{
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
    public IType? Type { get; set; }
}