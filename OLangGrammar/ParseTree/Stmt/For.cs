using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.Scope;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Stmt;

public class For(IdentifierToken identifier, IExpression start, IExpression end, IScopeNode scope) : IStatement
{
    public IdentifierToken Identifier = identifier;
    public IExpression Start = start;
    public IExpression End = end;
    public IScopeNode Scope = scope;
}