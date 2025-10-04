namespace OLangCompiler.Parser.Nodes.NodeValues;

public class VarDeclarationStatement(string identifier, TermNode term)
{
    public string Identifier = identifier;
    public TermNode Term = term;
}