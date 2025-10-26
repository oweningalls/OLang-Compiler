using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.Scope;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class While(BaseExpression condition, ScopeNode scope) : IStatement
{
    public BaseExpression Condition = condition;
    public ScopeNode Scope = scope;
}