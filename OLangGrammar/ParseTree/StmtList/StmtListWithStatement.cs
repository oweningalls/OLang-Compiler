using OLangGrammar.ParseTree.Stmt;

namespace OLangGrammar.ParseTree.StmtList;

public class StmtListWithStatement(IStatement statement, IStmtListNode stmtList) : IStmtListNode
{
    public IStatement Statement = statement;
    public IStmtListNode StmtList = stmtList;
}