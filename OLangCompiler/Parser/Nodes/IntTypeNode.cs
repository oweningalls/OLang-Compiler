using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.Nodes;

public class IntTypeNode : ITypeNode
{
    public ExpressionType Type => ExpressionType.Int;
}