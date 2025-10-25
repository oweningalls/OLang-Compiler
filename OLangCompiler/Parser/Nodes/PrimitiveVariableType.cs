using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class PrimitiveVariableType(BaseTypeToken type) : IVariableType
{
    public BaseTypeToken Type = type;
}
