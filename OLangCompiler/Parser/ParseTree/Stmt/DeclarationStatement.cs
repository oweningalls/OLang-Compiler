using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.VariableType;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class DeclarationStatement(IVariableType type, IdentifierToken identifier, BaseExpressionNode expression) : IStatementNode
{
    public IVariableType Type = type;
    public IdentifierToken Identifier = identifier;
    public BaseExpressionNode Expression = expression;
}