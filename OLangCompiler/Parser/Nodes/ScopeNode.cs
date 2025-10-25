namespace OLangCompiler.Parser.Nodes;

public class ScopeNode(IStmtListNode statementList) : INode
{
    public IStmtListNode StmtList = statementList;
}