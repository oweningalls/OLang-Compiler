namespace OLangCompiler.Tokens;

public class NotEqualToken : BaseBinaryOperatorToken
{
    public override int Precedence => 2;
}