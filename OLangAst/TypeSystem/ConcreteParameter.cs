namespace OLangAst.TypeSystem;

public class ConcreteParameter(string name, ConcreteType type)
{
    public readonly string Name = name;
    public readonly ConcreteType Type = type;
}