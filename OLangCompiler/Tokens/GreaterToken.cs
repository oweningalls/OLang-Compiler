namespace OLangCompiler.Tokens;

public class GreaterToken : BaseBinaryOperatorToken
{
    public override int Precedence => 3;
}