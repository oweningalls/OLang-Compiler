namespace OLangAst.TypeSystem;

public class ConcreteField(ConcreteType type, string name) : IField
{
    public readonly ConcreteType Type = type;
    public readonly string Name = name;
}