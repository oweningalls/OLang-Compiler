using Lexing;
using OLangAst.Expressions;

namespace OLangAst.Statements;

public class VariableAssignment(string identifier, IExpression value) : IStatement
{
    public string Identifier = identifier;
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
}