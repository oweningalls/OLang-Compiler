using Lexing;
using OLangAst.Miscellaneous;

namespace OLangAst.Expressions;

public class BoolLiteral(bool value) : IExpression
{
    public bool Value = value;
    public SourceSpan Span { get; set; }
    public IVariableType? Type { get; set; } = new PrimitiveVariableType(PrimitiveVariableTypeEnum.Bool);
}