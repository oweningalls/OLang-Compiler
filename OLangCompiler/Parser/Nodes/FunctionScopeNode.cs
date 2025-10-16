namespace OLangCompiler.Parser.Nodes;

public class FunctionScopeNode(List<IFunctionStatementNode> statements) : INode
{
    public List<IFunctionStatementNode> Statements = statements;
}