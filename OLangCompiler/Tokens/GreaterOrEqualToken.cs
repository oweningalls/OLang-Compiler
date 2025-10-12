namespace OLangCompiler.Tokens;

public class GreaterOrEqualToken : BaseBinaryOperatorToken
{
    public override int Precedence => 3;
}