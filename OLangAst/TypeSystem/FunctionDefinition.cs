namespace OLangAst.TypeSystem;

public struct FunctionDefinition(DefinedType declaringType, DefinedType returnType, string name, IEnumerable<Parameter> parameters, bool isInstance)
{
    public DefinedType DeclaringType = declaringType;
    public DefinedType ReturnType = returnType;
    public string Name = name;
    public IReadOnlyList<Parameter> Parameters = parameters.ToList();
    public bool IsInstance = isInstance;

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}