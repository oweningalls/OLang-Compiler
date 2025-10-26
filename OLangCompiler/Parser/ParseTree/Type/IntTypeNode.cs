using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.ParseTree.Type;

public class IntTypeNode : ITypeNode
{
    public ExpressionType Type => ExpressionType.Int;
}