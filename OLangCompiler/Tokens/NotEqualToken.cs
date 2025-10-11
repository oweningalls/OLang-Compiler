namespace OLangCompiler.Tokens;

public class NotEqualToken : IBinaryOperatorToken
{
    public int Precedence => 2;
}