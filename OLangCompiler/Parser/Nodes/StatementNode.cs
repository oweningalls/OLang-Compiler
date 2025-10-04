using OLangCompiler.Parser.Nodes.NodeValues;

namespace OLangCompiler.Parser.Nodes;

public class StatementNode(OneOf.OneOf<ExitStatement, VarDeclarationStatement> statement) : INode
{
    public OneOf.OneOf<ExitStatement, VarDeclarationStatement> Statement = statement;
}