using OLangCompiler.Parser.ParseTree.Stmt;

namespace OLangCompiler.Parser.ParseTree.StmtList;

public class StmtListWithStatement(IStatement statement, IStmtListNode stmtList) : IStmtListNode
{
    public IStatement Statement = statement;
    public IStmtListNode StmtList = stmtList;
}