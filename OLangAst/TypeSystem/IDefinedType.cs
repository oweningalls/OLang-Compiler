namespace OLangAst.TypeSystem;

public interface IDefinedType : IType
{
    public string Name { get; }
    public TypeVariant TypeVariant { get; }
    
    public Dictionary<string, IField> GetFields();
    public Dictionary<string, IEnumVariant> GetEnumVariants();
}
