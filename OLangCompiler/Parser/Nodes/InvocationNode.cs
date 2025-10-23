using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class InvocationNode(IdentifierToken identifier, ArgumentList argumentList) : BaseTermNode
{
    public IdentifierToken Identifier = identifier;
    public ArgumentList Arguments = argumentList;
}