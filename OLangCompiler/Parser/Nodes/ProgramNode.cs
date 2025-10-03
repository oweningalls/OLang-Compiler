namespace OLangCompiler.Parser.Nodes;

public class ProgramNode(Term value) : INode
{
    public Term Value = value;
}