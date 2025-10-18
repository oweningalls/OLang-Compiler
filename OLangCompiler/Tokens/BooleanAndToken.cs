namespace OLangCompiler.Tokens;

public class BooleanAndToken : BaseBinaryOperatorToken
{
    public override int Precedence => 1;
}