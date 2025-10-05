using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Tokens;

public class IntTypeToken : ITypeToken
{
    public ExpressionType? ExpType => ExpressionType.Int;
}