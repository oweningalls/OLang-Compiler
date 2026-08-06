using Lexing;

namespace OLangGrammar.ParseTree.StmtList;

public class EmptyStmtList : IStmtListNode
{
    public SourceSpan Span { get; set; }
}