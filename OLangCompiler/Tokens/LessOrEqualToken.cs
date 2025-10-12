namespace OLangCompiler.Tokens;

public class LessOrEqualToken: BaseBinaryOperatorToken
{
    public override int Precedence => 3;
}