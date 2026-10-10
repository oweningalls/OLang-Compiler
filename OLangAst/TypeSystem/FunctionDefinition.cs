namespace OLangAst.TypeSystem;

public struct FunctionDefinition(ConcreteType declaringType, ConcreteType returnType, string name, IEnumerable<Parameter> parameters, bool isInstance)
{
    public ConcreteType DeclaringType = declaringType;
    public ConcreteType ReturnType = returnType;
    public string Name = name;
    public IReadOnlyList<Parameter> Parameters = parameters.ToList();
    public bool IsInstance = isInstance;

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}