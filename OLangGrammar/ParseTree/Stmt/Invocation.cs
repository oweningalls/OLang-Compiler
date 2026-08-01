using OLangGrammar.ParseTree.FunctionInvocation;

namespace OLangGrammar.ParseTree.Stmt;

public class Invocation(IFunctionInvocation invocationNode) : IStatement
{
    public IFunctionInvocation InvocationNode = invocationNode;
}