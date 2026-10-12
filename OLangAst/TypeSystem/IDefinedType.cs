namespace OLangAst.TypeSystem;

public interface IDefinedType : IType
{
    public string Name { get; }
    public TypeVariant TypeVariant { get; }

    public List<FunctionDefinition> GetMethods();
    public Dictionary<string, FieldDefinition> GetFields();
    public Dictionary<string, DefinedEnumVariant> GetEnumVariants();
}
