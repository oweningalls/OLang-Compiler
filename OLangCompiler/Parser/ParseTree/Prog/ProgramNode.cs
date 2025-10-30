using OLangCompiler.Parser.ParseTree.StmtList;

namespace OLangCompiler.Parser.ParseTree.Prog;

public class ProgramNode(IStmtListNode stmtList) : IProgramNode
{
    public IStmtListNode StmtList = stmtList;
}