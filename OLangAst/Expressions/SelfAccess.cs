using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class SelfAccess : IExpression
{
    public SourceSpan Span { get; set; }
    public ConcreteType? Type { get; set; }
}