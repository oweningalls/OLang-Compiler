namespace OLangCompiler.Tokens;

public class IntLiteralToken(int value) : BaseToken
{
    public int Value = value;
}