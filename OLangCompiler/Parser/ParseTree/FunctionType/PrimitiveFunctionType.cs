using OLangCompiler.Parser.ParseTree.Type;

namespace OLangCompiler.Parser.ParseTree.FunctionType;

public class PrimitiveFunctionType(ITypeNode type) : IFunctionType
{
    public ITypeNode Type = type;
}