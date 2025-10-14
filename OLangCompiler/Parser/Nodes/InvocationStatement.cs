using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class InvocationStatement(IdentifierToken identifier) : IStatementNode
{
    public IdentifierToken Identifier = identifier;
}