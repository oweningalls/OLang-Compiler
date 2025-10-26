using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.Scope;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class While(IExpression condition, ScopeNode scope) : IStatement
{
    public IExpression Condition = condition;
    public ScopeNode Scope = scope;
}