using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.Nodes;

public interface ITypeNode : INode
{
    ExpressionType Type { get; }
}