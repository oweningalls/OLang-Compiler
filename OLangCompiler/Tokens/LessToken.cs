namespace OLangCompiler.Tokens;

public class LessToken : IBinaryOperatorToken
{
    public int Precedence => 3;
}