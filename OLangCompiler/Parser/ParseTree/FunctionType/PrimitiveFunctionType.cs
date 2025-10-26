using OLangCompiler.Parser.ParseTree.Type;

namespace OLangCompiler.Parser.ParseTree.FunctionType;

public class PrimitiveFunctionType(IType type) : IFunctionType
{
    public IType Type = type;
}