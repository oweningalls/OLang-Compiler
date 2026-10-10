namespace OLangAst.TypeSystem;

public class FunctionSignature(ConcreteType? returnType, IEnumerable<ConcreteType> parameterTypes)
{
    public readonly ConcreteType? ReturnType = returnType;
    public readonly List<ConcreteType> ParameterTypes = parameterTypes.ToList();
}