namespace OLangCompiler.Parser.Nodes;

public class ElseNode(ScopeNode scope) : IElseNode
{
    public ScopeNode Scope = scope;
}