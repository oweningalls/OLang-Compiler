using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class ArrayAccess(IExpression array, IExpression index) : IExpression
{
    public IExpression Array = array;
    public IExpression Index = index;
    public SourceSpan Span { get; set; }
    public ConcreteType? Type { get; set; }
}