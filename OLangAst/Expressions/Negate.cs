using Lexing;
using OLangAst.Miscellaneous;

namespace OLangAst.Expressions;

public class Negate(IExpression value) : IExpression
{
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
    public IVariableType? Type { get; set; }
}