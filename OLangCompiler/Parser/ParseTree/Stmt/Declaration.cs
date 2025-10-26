using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.VariableType;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class Declaration(IVariableType type, IdentifierToken identifier, BaseExpression expression) : IStatement
{
    public IVariableType Type = type;
    public IdentifierToken Identifier = identifier;
    public BaseExpression Expression = expression;
}