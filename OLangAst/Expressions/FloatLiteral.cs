using Lexing;
using OLangAst.Miscellaneous;

namespace OLangAst.Expressions;

public class FloatLiteral(float value) : IExpression
{
    public float Value = value;
    public SourceSpan Span { get; set; }
    public IVariableType? Type { get; set; } = new PrimitiveVariableType(PrimitiveVariableTypeEnum.Float);
}