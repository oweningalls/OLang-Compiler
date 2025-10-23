using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.Nodes;

public abstract class BaseValueNode : INode
{
    public ExpressionType? ValueType;
}