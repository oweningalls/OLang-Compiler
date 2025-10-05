using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Tokens;

public interface ITypeToken : IToken
{
    public ExpressionType? ExpType { get; }
}