using Lexing;

namespace OLangAst.Expressions;

public class BaseBinaryExpression(IExpression lhs, IExpression rhs) : IExpression
{
    public IExpression Lhs = lhs;
    public IExpression Rhs = rhs;
    public SourceSpan Span { get; set; }
}