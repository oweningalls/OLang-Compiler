using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class SetVarStatement(IdentifierToken identifier, IExpressionNode expression) : IStatementNode
{
    public IdentifierToken Identifier = identifier;
    public IExpressionNode Expression = expression;
}