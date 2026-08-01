using OLangAst.Miscellaneous;

namespace OLangAst.Statements;

public class Parameter(string identifier, IVariableType type)
{
    public IVariableType Type = type;
    public string Identifier = identifier;
}