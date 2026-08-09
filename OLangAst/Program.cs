using Lexing;
using OLangAst.Statements;

namespace OLangAst;

public class Program(List<IStatement> statements) : IAstNode
{
    public List<IStatement> Statements = statements;
    public SourceSpan Span { get; set; }
}