using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.Scope;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class While(IExpression condition, IScopeNode scope) : IStatement
{
    public IExpression Condition = condition;
    public IScopeNode Scope = scope;
}