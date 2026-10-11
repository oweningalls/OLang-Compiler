namespace OLangAst.TypeSystem;

public class FunctionDefinition(IType declaringType, IType returnType, string name, IEnumerable<DeclaredParameter> parameters, bool isInstance) : IFunctionDefinition
{
    public readonly IType DeclaringType = declaringType;
    public IType ReturnType = returnType;
    public string Name { get; } = name;
    public readonly IReadOnlyList<DeclaredParameter> Parameters = parameters.ToList();
    public bool IsInstance => isInstance;

    public IType GetReturnType()
    {
        return ReturnType;
    }
    
    public List<IType> GetParameterTypes()
    {
        return Parameters.Select(x => x.Type).ToList();
    }
    
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}