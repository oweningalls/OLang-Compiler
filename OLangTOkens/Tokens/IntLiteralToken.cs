using Lexing;

namespace OLangTokens.Tokens;

public class IntLiteralToken(int value) : BaseToken
{
    public int Value = value;
}