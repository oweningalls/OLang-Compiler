using Lexing;
using OLangAst.Expressions;

namespace OLangAst.Statements;

public class ForLoop(string identifier, IExpression rangeStart, IExpression rangeEnd, Scope body) : IStatement
{
    public string Identifier = identifier;
    public IExpression RangeStart = rangeStart;
    public IExpression RangeEnd = rangeEnd;
    public Scope Body = body;
    public SourceSpan Span { get; set; }
}