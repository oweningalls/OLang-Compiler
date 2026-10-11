namespace OLangAst.TypeSystem;

public class DeclaredParameter(string name, IType type)
{
    public readonly string Name = name;
    public readonly IType Type = type;
}