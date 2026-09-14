using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class StringLiteral(string value) : IExpression
{
    public string Value = value;
    public SourceSpan Span { get; set; }
    public DefinedType? Type { get; set; } = PrimitiveTypes.StringType;
}