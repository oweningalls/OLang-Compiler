namespace OLangAst.TypeSystem;

public class FunctionSignature(DefinedType? returnType, IEnumerable<DefinedType> parameterTypes)
{
    public readonly DefinedType? ReturnType = returnType;
    public readonly List<DefinedType> ParameterTypes = parameterTypes.ToList();
}