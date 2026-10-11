namespace OLangAst.TypeSystem;

public class GenericType(string typeParameterName) : IType
{
    public readonly string TypeParameterName = typeParameterName;
}