using OLangCompiler.Parser.ParseTree.StmtList;

namespace OLangCompiler.Parser.ParseTree.Scope;

public class ScopeNode(IStmtListNode statementList) : INode
{
    public IStmtListNode StmtList = statementList;
}