using Lexing;
using OLangAst.Expressions;

namespace OLangAst.Statements;

public class Return(IExpression? value) : IStatement
{
    public IExpression? Value = value;
    public SourceSpan Span { get; set; }
}