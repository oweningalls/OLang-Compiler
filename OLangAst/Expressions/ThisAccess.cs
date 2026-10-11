using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class ThisAccess : IExpression
{
    public SourceSpan Span { get; set; }
    public IType? Type { get; set; }
}