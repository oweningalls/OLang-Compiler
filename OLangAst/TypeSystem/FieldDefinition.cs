namespace OLangAst.TypeSystem;

public class FieldDefinition(IType type, string name) : IField
{
    public readonly IType Type = type;
    public readonly string Name = name;
}