namespace OLangAst.TypeSystem;

public class DefinedEnumVariant(string name, List<IType> parameters) : IEnumVariant
{
    public string Name = name;
    public List<IType> Parameters = parameters;
    public List<IType> GetParameters()
    {
        return Parameters;
    }
}