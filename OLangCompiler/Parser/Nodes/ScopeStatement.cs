namespace OLangCompiler.Parser.Nodes;

public class ScopeStatement(ScopeNode scope) : IStatementNode
{
    public ScopeNode Scope = scope;
}