namespace OLangCompiler.Tokens;

public class MinusToken : BaseBinaryOperatorToken
{
    public override int Precedence => 4;
}