namespace OLangCompiler.Parser.ParseTree.Term;

public class InvocationTerm(InvocationNode invocationNode) : BaseTermNode
{
    public InvocationNode InvocationNode = invocationNode;
}