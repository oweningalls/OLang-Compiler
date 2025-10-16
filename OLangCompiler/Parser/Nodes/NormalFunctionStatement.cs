namespace OLangCompiler.Parser.Nodes;

public class NormalFunctionStatement(IStatementNode statement) : IFunctionStatementNode
{
    public IStatementNode Statement = statement;
}