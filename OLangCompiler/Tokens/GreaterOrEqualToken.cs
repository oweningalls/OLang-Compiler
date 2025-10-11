namespace OLangCompiler.Tokens;

public class GreaterOrEqualToken : IBinaryOperatorToken
{
    public int Precedence => 3;
}