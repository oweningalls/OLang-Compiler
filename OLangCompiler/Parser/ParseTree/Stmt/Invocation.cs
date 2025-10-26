namespace OLangCompiler.Parser.ParseTree.Stmt;

public class Invocation(FunctionInvocation.FunctionInvocation invocationNode) : IStatement
{
    public FunctionInvocation.FunctionInvocation InvocationNode = invocationNode;
}