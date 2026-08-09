using Lexing;
using OLangGrammar.ParseTree.Stmt;

namespace OLangGrammar.ParseTree.StmtList;

public class SingleStatementStmtList(IStatement statement) : IStmtListNode
{
    public IStatement Statement = statement;
    public SourceSpan Span { get; set; }
}