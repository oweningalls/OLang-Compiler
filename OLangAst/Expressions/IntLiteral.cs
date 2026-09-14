using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class IntLiteral(int value) : IExpression
{
    public int Value = value;
    public SourceSpan Span { get; set; }
    public DefinedType? Type { get; set; } = PrimitiveTypes.IntType;
}
