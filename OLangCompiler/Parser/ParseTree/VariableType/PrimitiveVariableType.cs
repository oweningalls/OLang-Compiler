using OLangCompiler.Parser.ParseTree.Type;

namespace OLangCompiler.Parser.ParseTree.VariableType;

public class PrimitiveVariableType(IType type) : IVariableType
{
    public IType Type = type;
}
