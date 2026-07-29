using OLangAst.Expressions;
using OLangAst.Miscellaneous;

namespace OLangAst.Statements;

public class VariableDeclarationStatement : IStatement
{
    public IVariableType? Type;
    public string Identifier;
    public IExpression Value;
}