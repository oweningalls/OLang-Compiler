namespace OLangCompiler.Parser.ParseTree.Term;

public class IdentifierTerm(string identifier) : BaseTermNode
{
    public string Identifier = identifier;
}