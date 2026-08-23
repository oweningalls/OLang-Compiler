using OLangAst.Miscellaneous;

namespace OLangAst.Expressions;

public interface IExpression : IAstNode
{
    IVariableType? Type { get; set; }
}
