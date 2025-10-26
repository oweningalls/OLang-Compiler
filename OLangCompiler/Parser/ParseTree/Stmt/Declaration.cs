using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.VariableType;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class Declaration(IVariableType type, IdentifierToken identifier, IExpression expression) : IStatement
{
    public IVariableType Type = type;
    public IdentifierToken Identifier = identifier;
    public IExpression Expression = expression;
}