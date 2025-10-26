using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.ParseTree.Type;

public interface ITypeNode : INode
{
    ExpressionType Type { get; }
}