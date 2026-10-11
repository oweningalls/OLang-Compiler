using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public interface IExpression : IAstNode
{
    IType? Type { get; set; }
}
