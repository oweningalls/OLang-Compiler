using OLangAst.Miscellaneous;

namespace OLangAst.Statements;

public class FunctionDeclaration : IStatement
{
    public IVariableType? Type;
    public string Identifier;
    public List<Parameter> Parameters;
}