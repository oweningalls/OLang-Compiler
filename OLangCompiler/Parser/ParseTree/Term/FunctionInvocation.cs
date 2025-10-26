namespace OLangCompiler.Parser.ParseTree.Term;

public class FunctionInvocation(InvocationNode invocationNode) : ITerm
{
    public InvocationNode InvocationNode = invocationNode;
}