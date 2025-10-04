namespace OLangCompiler.Parser.Nodes;

public class ProgramNode(List<IStatementNode> statements) : INode
{
    public List<IStatementNode> Statements = statements;
}