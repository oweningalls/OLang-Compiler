using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class FloatLiteral(float value) : IExpression
{
    public float Value = value;
    public SourceSpan Span { get; set; }
    public DefinedType? Type { get; set; } = PrimitiveTypes.FloatType;
}