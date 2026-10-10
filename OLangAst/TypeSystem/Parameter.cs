namespace OLangAst.TypeSystem;

public struct Parameter(string name, ConcreteType type)
{
    public string Name = name;
    public ConcreteType Type = type;
}