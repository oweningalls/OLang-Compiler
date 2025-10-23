namespace OLangCompiler.Parser.Nodes;

public class ArgumentList(List<BaseExpressionNode> expressions) : INode
{
    public List<BaseExpressionNode> Expressions = expressions;
}