namespace OLangCompiler.Tokens;

public class PlusToken : IBinaryOperatorToken
{
    public int Precedence => 1;
}