using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class InvocationNode(IdentifierToken identifier) : INode
{
    public IdentifierToken Identifier = identifier;
}