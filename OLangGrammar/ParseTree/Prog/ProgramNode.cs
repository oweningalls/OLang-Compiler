using OLangGrammar.ParseTree.StmtList;

namespace OLangGrammar.ParseTree.Prog;

public class ProgramNode(IStmtListNode stmtList) : IProgramNode
{
    public IStmtListNode StmtList = stmtList;
}