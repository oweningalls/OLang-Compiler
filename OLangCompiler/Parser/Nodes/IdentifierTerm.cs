namespace OLangCompiler.Parser.Nodes;

public class IdentifierTerm(string identifier) : ITermNode
{
    public string Identifier = identifier;
}