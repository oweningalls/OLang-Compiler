using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.Nodes;

public class BoolTypeNode : ITypeNode
{
    public ExpressionType Type => ExpressionType.Bool;
}