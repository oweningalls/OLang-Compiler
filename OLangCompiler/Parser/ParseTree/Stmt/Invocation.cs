using OLangCompiler.Parser.ParseTree.FunctionInvocation;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class Invocation(IFunctionInvocation invocationNode) : IStatement
{
    public IFunctionInvocation InvocationNode = invocationNode;
}