namespace OLangCompiler.Parser.Nodes;

public class StmtListWithStatement(IStatementNode statement, IStmtListNode stmtList) : IStmtListNode
{
    public IStatementNode Statement = statement;
    public IStmtListNode StmtList = stmtList;
}