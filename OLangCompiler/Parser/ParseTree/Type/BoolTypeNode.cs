using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.ParseTree.Type;

public class BoolTypeNode : ITypeNode
{
    public ExpressionType Type => ExpressionType.Bool;
}