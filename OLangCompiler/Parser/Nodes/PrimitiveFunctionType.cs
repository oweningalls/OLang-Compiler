using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class PrimitiveFunctionType(BaseTypeToken type) : IFunctionType
{
    public BaseTypeToken Type = type;
}