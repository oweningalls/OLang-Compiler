namespace OLangCompiler.Tokens;

public class MinusToken : IBinaryOperatorToken
{
    public int Precedence => 1;
}