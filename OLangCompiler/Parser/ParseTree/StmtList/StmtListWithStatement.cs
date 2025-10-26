using OLangCompiler.Parser.ParseTree.Stmt;

namespace OLangCompiler.Parser.ParseTree.StmtList;

public class StmtListWithStatement(IStatementNode statement, IStmtListNode stmtList) : IStmtListNode
{
    public IStatementNode Statement = statement;
    public IStmtListNode StmtList = stmtList;
}