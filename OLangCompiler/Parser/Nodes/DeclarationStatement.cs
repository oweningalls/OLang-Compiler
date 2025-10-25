using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.Nodes;

public class DeclarationStatement(IVariableType type, IdentifierToken identifier, BaseExpressionNode expression) : IStatementNode
{
    public IVariableType Type = type;
    public IdentifierToken Identifier = identifier;
    public BaseExpressionNode Expression = expression;
}