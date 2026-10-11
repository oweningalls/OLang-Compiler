namespace OLangAst.TypeSystem;

public class ConcreteFunctionDefinition(ConcreteType declaringType, ConcreteType returnType, string name, IEnumerable<ConcreteParameter> parameters, bool isInstance) : IFunctionDefinition
{
    public readonly ConcreteType DeclaringType = declaringType;
    public readonly ConcreteType ReturnType = returnType;
    public string Name { get; } = name;
    public readonly IReadOnlyList<ConcreteParameter> Parameters = parameters.ToList();
    public bool IsInstance { get; } = isInstance;

    public IType GetReturnType()
    {
        return ReturnType;
    }

    public List<IType> GetParameterTypes()
    {
        return Parameters.Select(x => x.Type).Cast<IType>().ToList();
    }
    
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}