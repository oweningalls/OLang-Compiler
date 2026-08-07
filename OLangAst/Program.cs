using Lexing;
using OLangAst.Statements;

namespace OLangAst;

public class Program : IAstNode
{
    public List<IStatement> Statements;
    public SourceSpan Span { get; set; }
}