using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public interface IExpression : IAstNode
{
    DefinedType? Type { get; set; }
}
