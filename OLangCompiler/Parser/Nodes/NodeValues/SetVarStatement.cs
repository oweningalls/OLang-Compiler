using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes.NodeValues;

public class SetVarStatement(IdentifierToken identifier, TermNode term)
{
    public IdentifierToken Identifier = identifier;
    public TermNode Term = term;
}