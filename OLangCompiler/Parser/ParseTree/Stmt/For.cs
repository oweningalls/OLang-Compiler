using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.Scope;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class For(IdentifierToken identifier, IExpression start, IExpression end, ScopeNode scope) : IStatement
{
    public IdentifierToken Identifier = identifier;
    public IExpression Start = start;
    public IExpression End = end;
    public ScopeNode Scope = scope;
}