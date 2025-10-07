namespace OLangCompiler.Parser.Nodes;

public class ScopeNode(List<IStatementNode> statements) : INode
{
    public List<IStatementNode> Statements = statements;
}