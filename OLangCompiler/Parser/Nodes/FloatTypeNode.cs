using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.Nodes;

public class FloatTypeNode : ITypeNode
{
    public ExpressionType Type => ExpressionType.Float;
}