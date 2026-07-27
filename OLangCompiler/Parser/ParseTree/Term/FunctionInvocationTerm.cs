namespace OLangCompiler.Parser.ParseTree.Term;

public class FunctionInvocationTerm(FunctionInvocation.IFunctionInvocation invocationNode) : ITerm
{
    public FunctionInvocation.IFunctionInvocation InvocationNode = invocationNode;
}