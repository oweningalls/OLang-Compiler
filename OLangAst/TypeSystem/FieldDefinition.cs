namespace OLangAst.TypeSystem;

public class FieldDefinition(IType type, string name)
{
    public readonly IType Type = type;
    public readonly string Name = name;
}