namespace OLangCompiler.Tokens;

public abstract class BaseBinaryOperatorToken : BaseToken
{
    public abstract int Precedence { get; }
}