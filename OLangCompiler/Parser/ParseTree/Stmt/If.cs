using OLangCompiler.Parser.ParseTree.ElseBlock;
using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.Scope;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class If(BaseExpression condition, ScopeNode scope, IElse elseBlock) : IStatement
{
    public BaseExpression Condition = condition;
    public ScopeNode Scope = scope;
    public IElse ElseBlock = elseBlock;
}