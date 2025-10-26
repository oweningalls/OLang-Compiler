namespace OLangCompiler.Parser.ParseTree.Stmt;

public class InvocationStatement(InvocationNode invocationNode) : IStatementNode
{
    public InvocationNode InvocationNode = invocationNode;
}