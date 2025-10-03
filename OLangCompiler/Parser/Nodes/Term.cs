namespace OLangCompiler.Parser.Nodes;

public class Term(OneOf.OneOf<int> value) : INode
{
    public OneOf.OneOf<int> Value = value;
}