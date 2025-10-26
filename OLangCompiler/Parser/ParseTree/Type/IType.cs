using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.ParseTree.Type;

public interface IType : INode
{
    ExpressionType Type { get; }
}