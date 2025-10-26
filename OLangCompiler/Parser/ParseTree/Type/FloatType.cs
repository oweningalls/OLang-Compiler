using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.ParseTree.Type;

public class FloatType : IType
{
    public ExpressionType Type => ExpressionType.Float;
}