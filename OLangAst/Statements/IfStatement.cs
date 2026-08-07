using Lexing;
using OLangAst.Expressions;

namespace OLangAst.Statements;

public class IfStatement(IExpression predicate, Scope body, Scope? @else) : IStatement
{
    public IExpression Predicate = predicate;
    public Scope Body = body;
    public Scope? Else = @else;
    public SourceSpan Span { get; set; }
}