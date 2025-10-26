using OLangCompiler.Parser.ParseTree.Expression;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class ReturnValue(IExpression expression) : IStatement
{
    public IExpression Expression = expression;
}