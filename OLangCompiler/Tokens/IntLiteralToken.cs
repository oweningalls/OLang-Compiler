namespace OLangCompiler.Tokens;

public class IntLiteralToken(int value) : IToken
{
    public int Value = value;
}