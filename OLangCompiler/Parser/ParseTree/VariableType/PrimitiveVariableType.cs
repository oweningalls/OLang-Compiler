using OLangCompiler.Parser.ParseTree.Type;

namespace OLangCompiler.Parser.ParseTree.VariableType;

public class PrimitiveVariableType(ITypeNode type) : IVariableType
{
    public ITypeNode Type = type;
}
