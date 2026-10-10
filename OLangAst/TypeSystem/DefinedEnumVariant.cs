namespace OLangAst.TypeSystem;

public struct DefinedEnumVariant(string name, List<ConcreteType> parameters)
{
    public string Name = name;
    public List<ConcreteType> Parameters = parameters;
}