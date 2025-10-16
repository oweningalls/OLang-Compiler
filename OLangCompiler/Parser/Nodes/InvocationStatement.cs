using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class InvocationStatement(InvocationNode invocationNode) : IStatementNode
{
    public InvocationNode InvocationNode = invocationNode;
}