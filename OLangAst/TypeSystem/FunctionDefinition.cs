namespace OLangAst.TypeSystem;

public struct FunctionDefinition(DefinedType declaringType, DefinedType returnType, string name, IEnumerable<Parameter> parameters)
{
    public DefinedType DeclaringType = declaringType;
    public DefinedType ReturnType = returnType;
    public string Name = name;
    public IReadOnlyList<Parameter> Parameters = parameters.ToList();

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}