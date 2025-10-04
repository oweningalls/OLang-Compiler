namespace OLangCompiler.Parser.Nodes;

public class ProgramNode(List<StatementNode> statements) : INode
{
    public List<StatementNode> Statements = statements;
}