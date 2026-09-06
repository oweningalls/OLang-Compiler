using OLangAst.Miscellaneous;

namespace AstHelpers;

public class FunctionSignature(IVariableType? returnType, IEnumerable<IVariableType> parameterTypes)
{
    public readonly IVariableType? ReturnType = returnType;
    public readonly List<IVariableType> ParameterTypes = parameterTypes.ToList();
}