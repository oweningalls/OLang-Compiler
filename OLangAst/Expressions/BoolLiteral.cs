using Lexing;

namespace OLangAst.Expressions;

public class BoolLiteral(bool value) : IExpression
{
    public bool Value = value;
    public SourceSpan Span { get; set; }
}