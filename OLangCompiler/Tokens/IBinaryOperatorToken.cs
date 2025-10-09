namespace OLangCompiler.Tokens;

public interface IBinaryOperatorToken : IToken
{
    public int Precedence { get; }
}