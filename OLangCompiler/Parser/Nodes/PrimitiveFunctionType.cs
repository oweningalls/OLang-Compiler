using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class PrimitiveFunctionType(ITypeNode type) : IFunctionType
{
    public ITypeNode Type = type;
}