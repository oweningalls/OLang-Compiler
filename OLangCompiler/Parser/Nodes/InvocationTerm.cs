namespace OLangCompiler.Parser.Nodes;

public class InvocationTerm(InvocationNode invocationNode) : ITermNode
{
    public InvocationNode InvocationNode = invocationNode;
}