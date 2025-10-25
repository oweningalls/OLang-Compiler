namespace OLangCompiler.Parser.Nodes;

public class ProgramNode(IStmtListNode stmtList) : INode
{
    public IStmtListNode StmtList = stmtList;
}