namespace OLangCompiler.Parser.Nodes;

public class ElseNode(ScopeNode scope) : INode
{
    public ScopeNode Scope = scope;
}