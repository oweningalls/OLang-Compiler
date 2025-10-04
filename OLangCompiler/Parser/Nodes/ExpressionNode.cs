using OLangCompiler.Parser.Nodes.NodeValues;
using OneOf;

namespace OLangCompiler.Parser.Nodes;

public class ExpressionNode(OneOf<AddExpression, TermNode> expression) : INode
{
    public OneOf<AddExpression, TermNode> Expression = expression;
}