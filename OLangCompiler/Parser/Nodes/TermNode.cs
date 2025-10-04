namespace OLangCompiler.Parser.Nodes;

public class TermNode(OneOf.OneOf<int> value) : INode
{
    public OneOf.OneOf<int> Value = value;
}