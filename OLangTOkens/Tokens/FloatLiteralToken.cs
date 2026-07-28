using Lexing;

namespace OLangTokens.Tokens;

public class FloatLiteralToken(float value) : BaseToken
{
    public float Value = value;
}