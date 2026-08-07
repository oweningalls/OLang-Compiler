using Lexing;

namespace OLangAst.Expressions;

public class FloatLiteral(float value) : IExpression
{
    public float Value = value;
    public SourceSpan Span { get; set; }
}