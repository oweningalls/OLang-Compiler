using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class FieldAccess(IExpression expression, string fieldName) : IExpression
{
    public IExpression Expression = expression;
    public string FieldName = fieldName;
    public SourceSpan Span { get; set; }
    public DefinedType? Type { get; set; }
}