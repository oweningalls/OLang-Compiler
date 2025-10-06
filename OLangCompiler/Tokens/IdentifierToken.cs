namespace OLangCompiler.Tokens;

public class IdentifierToken(string identifier) : IToken
{
    public string Identifier = identifier;
}