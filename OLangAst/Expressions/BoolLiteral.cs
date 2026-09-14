using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class BoolLiteral(bool value) : IExpression
{
    public bool Value = value;
    public SourceSpan Span { get; set; }
    public DefinedType? Type { get; set; } = PrimitiveTypes.BoolType;
}