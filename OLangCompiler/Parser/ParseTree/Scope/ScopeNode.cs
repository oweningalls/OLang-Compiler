using OLangCompiler.Parser.ParseTree.StmtList;

namespace OLangCompiler.Parser.ParseTree.Scope;

public class ScopeNode(IStmtListNode statementList) : IScopeNode
{
    public IStmtListNode StmtList = statementList;
}