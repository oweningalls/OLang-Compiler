namespace OLangAst.TypeSystem;

public class ConcreteEnumVariant(string name, List<ConcreteType> parameters) : IEnumVariant
{
    public readonly string Name = name;
    public readonly List<ConcreteType> Parameters = parameters;
    
    public List<IType> GetParameters()
    {
        return Parameters.Cast<IType>().ToList();
    }
}