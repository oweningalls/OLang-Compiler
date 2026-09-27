namespace OLangAst.TypeSystem;

public struct FieldDefinition(DefinedType type, string name)
{
    public readonly DefinedType Type = type;
    public readonly string Name = name;
}