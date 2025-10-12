using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Tokens;

public class BoolTypeToken : BaseTypeToken
{
    public override ExpressionType? ExpType => ExpressionType.Bool;
}