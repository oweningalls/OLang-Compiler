namespace OLangCompiler.Tokens;

public class PlusToken : BaseBinaryOperatorToken
{
    public override int Precedence => 3;
}