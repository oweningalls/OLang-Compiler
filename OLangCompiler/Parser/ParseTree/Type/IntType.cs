using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.ParseTree.Type;

public class IntType : IType
{
    public ExpressionType Type => ExpressionType.Int;
}