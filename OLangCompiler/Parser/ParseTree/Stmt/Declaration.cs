using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.Type;
using OLangTokens.Tokens;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class Declaration(IType type, IdentifierToken identifier, IExpression expression) : IStatement
{
    public IType Type = type;
    public IdentifierToken Identifier = identifier;
    public IExpression Expression = expression;
}