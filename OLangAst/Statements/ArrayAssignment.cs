using Lexing;
using OLangAst.Expressions;

namespace OLangAst.Statements;

public class ArrayAssignment(IExpression array, IExpression index, IExpression value) : IStatement
{
    public IExpression Array = array;
    public IExpression Index = index;
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
}