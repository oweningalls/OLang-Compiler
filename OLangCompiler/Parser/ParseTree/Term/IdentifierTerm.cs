namespace OLangCompiler.Parser.ParseTree.Term;

public class IdentifierTerm(string identifier) : ITerm
{
    public string Identifier = identifier;
}