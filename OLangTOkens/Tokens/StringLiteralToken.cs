using Lexing;

namespace OLangTokens.Tokens;

public class StringLiteralToken(string value) : BaseToken
{
    public string Value = value;
}