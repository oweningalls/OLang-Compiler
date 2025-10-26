using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.Scope;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class For(IdentifierToken identifier, BaseExpression start, BaseExpression end, ScopeNode scope) : IStatement
{
    public IdentifierToken Identifier = identifier;
    public BaseExpression Start = start;
    public BaseExpression End = end;
    public ScopeNode Scope = scope;
}