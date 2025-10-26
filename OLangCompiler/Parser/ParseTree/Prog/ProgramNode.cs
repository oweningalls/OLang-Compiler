using OLangCompiler.Parser.ParseTree.StmtList;

namespace OLangCompiler.Parser.ParseTree.Prog;

public class ProgramNode(IStmtListNode stmtList) : INode
{
    public IStmtListNode StmtList = stmtList;
}