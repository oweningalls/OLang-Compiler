using Lexing;
using OLangGrammar.ParseTree.StmtList;

namespace OLangGrammar.ParseTree.Scope;

public class ScopeNode(IStmtListNode statementList) : IScopeNode
{
    public IStmtListNode StmtList = statementList;
    public SourceSpan Span { get; set; }
}