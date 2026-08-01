using OLangGrammar.ParseTree.Expression;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Stmt;

public class LetDeclaration(IdentifierToken identifier, IExpression expression) : IStatement
{
    public IdentifierToken Identifier = identifier;
    public IExpression Expression = expression;
}