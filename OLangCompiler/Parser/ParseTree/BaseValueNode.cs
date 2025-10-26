using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.ParseTree;

public abstract class BaseValueNode : INode
{
    public ExpressionType? ValueType;
}