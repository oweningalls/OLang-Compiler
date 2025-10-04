namespace OLangCompiler.Tokens;

public class IdentifierToken(string name) : IToken
{
    public string Name = name;
}