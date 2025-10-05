using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Tokens;

public class BoolTypeToken : ITypeToken
{
    public ExpressionType? ExpType => ExpressionType.Bool;
}