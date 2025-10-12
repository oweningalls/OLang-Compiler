namespace OLangCompiler.Tokens;

public class IdentifierToken(string identifier) : BaseToken
{
    public string Identifier = identifier;
}