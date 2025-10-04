using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes.NodeValues;

public class SetVarStatement(IdentifierToken identifier, ExpressionNode expression)
{
    public IdentifierToken Identifier = identifier;
    public ExpressionNode Expression = expression;
}