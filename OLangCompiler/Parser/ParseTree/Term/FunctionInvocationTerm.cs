namespace OLangCompiler.Parser.ParseTree.Term;

public class FunctionInvocationTerm(ParseTree.FunctionInvocation.FunctionInvocation invocationNode) : ITerm
{
    public ParseTree.FunctionInvocation.FunctionInvocation InvocationNode = invocationNode;
}