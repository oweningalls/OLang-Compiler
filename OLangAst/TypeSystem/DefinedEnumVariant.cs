namespace OLangAst.TypeSystem;

public struct DefinedEnumVariant(string name, List<DefinedType> parameters)
{
    public string Name = name;
    public List<DefinedType> Parameters = parameters;
}