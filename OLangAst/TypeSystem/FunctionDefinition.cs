namespace OLangAst.TypeSystem;

public struct FunctionDefinition(DefinedType returnType, string name, IEnumerable<Parameter> parameters)
{
    public DefinedType ReturnType = returnType;
    public string Name = name;
    public IReadOnlyList<Parameter> Parameters = parameters.ToList();

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}