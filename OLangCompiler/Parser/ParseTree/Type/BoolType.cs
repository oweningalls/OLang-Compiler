using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.ParseTree.Type;

public class BoolType : IType
{
    public ExpressionType Type => ExpressionType.Bool;
}