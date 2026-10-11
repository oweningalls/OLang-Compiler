namespace OLangAst.TypeSystem;

public class FunctionSignature(IType? returnType, IEnumerable<IType> parameterTypes)
{
    public readonly IType? ReturnType = returnType;
    public readonly List<IType> ParameterTypes = parameterTypes.ToList();
}