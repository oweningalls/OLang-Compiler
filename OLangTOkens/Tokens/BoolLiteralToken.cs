using Lexing;

namespace OLangTokens.Tokens;

public class BoolLiteralToken(bool value) : BaseToken
{
    public bool Value = value;
}