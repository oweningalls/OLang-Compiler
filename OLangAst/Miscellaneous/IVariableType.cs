namespace OLangAst.Miscellaneous;

public interface IVariableType : IAstNode
{
    string Name { get; }
    bool IsArray { get; }
}
