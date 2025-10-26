using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.ParseTree.Type;

public class FloatTypeNode : ITypeNode
{
    public ExpressionType Type => ExpressionType.Float;
}