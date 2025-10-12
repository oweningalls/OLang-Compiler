using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Tokens;

public abstract class BaseTypeToken : BaseToken
{
    public abstract ExpressionType? ExpType { get; }
}