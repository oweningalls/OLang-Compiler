using OLangAst.Miscellaneous;

namespace OLangTypeChecking.TypeChecking.Types;

public class FunctionSignature(IVariableType? returnType, IEnumerable<IVariableType> parameterTypes)
{
    public readonly IVariableType? ReturnType = returnType;
    public readonly List<IVariableType> ParameterTypes = parameterTypes.ToList();
}