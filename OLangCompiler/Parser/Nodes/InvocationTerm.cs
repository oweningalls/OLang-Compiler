namespace OLangCompiler.Parser.Nodes;

public class InvocationTerm(InvocationNode invocationNode) : BaseTermNode
{
    public InvocationNode InvocationNode = invocationNode;
}