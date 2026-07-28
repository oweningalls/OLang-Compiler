using OLangCompiler.Parser.ParseTree.Expression;
using OLangTokens.Tokens;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class LetDeclaration(IdentifierToken identifier, IExpression expression) : IStatement
{
    public IdentifierToken Identifier = identifier;
    public IExpression Expression = expression;
}