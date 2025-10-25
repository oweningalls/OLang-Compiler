using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class PrimitiveVariableType(ITypeNode type) : IVariableType
{
    public ITypeNode Type = type;
}
