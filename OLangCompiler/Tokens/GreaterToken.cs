namespace OLangCompiler.Tokens;

public class GreaterToken : IBinaryOperatorToken
{
    public int Precedence => 3;
}