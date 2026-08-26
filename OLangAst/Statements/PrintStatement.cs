using Lexing;
using OLangAst.Expressions;

namespace OLangAst.Statements;

public class PrintStatement(IExpression expression) : IStatement
{
    public IExpression Expression = expression;
    public SourceSpan Span { get; set; }
}