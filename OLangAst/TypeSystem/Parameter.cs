namespace OLangAst.TypeSystem;

public struct Parameter(string name, DefinedType type)
{
    public string Name = name;
    public DefinedType Type = type;
}