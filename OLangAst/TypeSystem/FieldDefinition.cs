namespace OLangAst.TypeSystem;

public struct FieldDefinition(ConcreteType type, string name)
{
    public readonly ConcreteType Type = type;
    public readonly string Name = name;
}