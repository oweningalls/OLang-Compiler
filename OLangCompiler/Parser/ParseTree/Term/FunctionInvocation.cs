namespace OLangCompiler.Parser.ParseTree.Term;

public class FunctionInvocation(InvocationNode invocationNode) : BaseTermNode
{
    public InvocationNode InvocationNode = invocationNode;
}