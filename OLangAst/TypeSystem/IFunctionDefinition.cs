namespace OLangAst.TypeSystem;

public interface IFunctionDefinition
{
    public string Name { get; }
    public bool IsInstance { get; }
    public List<IType> GetParameterTypes();
    public IType GetReturnType();
}