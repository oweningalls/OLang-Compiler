namespace OLangCompiler.Parser.ParseTree.Term;

public class FunctionInvocation(ParseTree.FunctionInvocation.FunctionInvocation invocationNode) : ITerm
{
    public ParseTree.FunctionInvocation.FunctionInvocation InvocationNode = invocationNode;
}