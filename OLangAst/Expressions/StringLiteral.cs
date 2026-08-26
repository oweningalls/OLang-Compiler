using Lexing;
using OLangAst.Miscellaneous;

namespace OLangAst.Expressions;

public class StringLiteral(string value) : IExpression
{
    public string Value = value;
    public SourceSpan Span { get; set; }
    public IVariableType? Type { get; set; } = new PrimitiveVariableType(PrimitiveVariableTypeEnum.String);
}