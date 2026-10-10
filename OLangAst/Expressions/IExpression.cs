using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public interface IExpression : IAstNode
{
    ConcreteType? Type { get; set; }
}
