namespace OLangCompiler.Tokens;

public class BoolLiteralToken(bool value) : IToken
{
    public bool Value = value;
}