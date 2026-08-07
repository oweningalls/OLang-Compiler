using Lexing;

namespace OLangAst.Statements;

public class Scope(List<IStatement> statements) : IStatement
{
    public List<IStatement> Statements = statements;
    public SourceSpan Span { get; set; }
}