using OLangCompiler.Parser.ParseTree.ElseBlock;
using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.Scope;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class If(IExpression condition, IScopeNode scope, IElse elseBlock) : IStatement
{
    public IExpression Condition = condition;
    public IScopeNode Scope = scope;
    public IElse ElseBlock = elseBlock;
}