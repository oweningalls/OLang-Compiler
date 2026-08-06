using Lexing;

namespace OLangGrammar.ParseTree.Stmt;

public class Return : IStatement
{
    public SourceSpan Span { get; set; }
}