using Lexing;
using OLangAst.Miscellaneous;

namespace OLangAst.Expressions;

public class IntLiteral(int value) : IExpression
{
    public int Value = value;
    public SourceSpan Span { get; set; }
    public IVariableType? Type { get; set; } = new PrimitiveVariableType(PrimitiveVariableTypeEnum.Int);
}
