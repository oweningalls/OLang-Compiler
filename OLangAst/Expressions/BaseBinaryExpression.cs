using Lexing;
using OLangAst.Miscellaneous;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public abstract class BaseBinaryExpression(IExpression lhs, IExpression rhs) : IExpression
{
    public IExpression Lhs = lhs;
    public IExpression Rhs = rhs;
    public SourceSpan Span { get; set; }
    public abstract ConcreteType? Type { get; set; }
}