using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Tokens;

public class IntTypeToken : BaseTypeToken
{
    public override ExpressionType? ExpType => ExpressionType.Int;
}