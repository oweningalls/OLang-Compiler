namespace OLangCompiler.Parser.Nodes;

public class IdentifierTerm(string identifier) : BaseTermNode
{
    public string Identifier = identifier;
}