namespace OLangCompiler.Parser.Nodes;

public class ArgumentList(List<IExpressionNode> expressions) : INode
{
    public List<IExpressionNode> Expressions = expressions;
}