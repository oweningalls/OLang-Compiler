namespace OLangCompiler.Parser.ParseTree.Stmt;

public class Invocation(InvocationNode invocationNode) : IStatement
{
    public InvocationNode InvocationNode = invocationNode;
}