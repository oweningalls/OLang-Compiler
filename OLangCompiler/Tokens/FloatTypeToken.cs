using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Tokens;

public class FloatTypeToken : BaseTypeToken
{
    public override ExpressionType? ExpType => ExpressionType.Float;
}