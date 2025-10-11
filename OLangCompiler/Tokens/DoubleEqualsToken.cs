namespace OLangCompiler.Tokens;

public class DoubleEqualsToken : IBinaryOperatorToken
{
    public int Precedence => 2;
}