using Lexing;
using OLangAst.Expressions;

namespace OLangAst.Statements;

public class FieldAssignment(IExpression target, string identifier, IExpression value) : IStatement
{
    public IExpression Target = target;
    public string Identifier = identifier;
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
}