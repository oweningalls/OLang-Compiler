using OLangCompiler.Parser.ParseTree.Expression;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class Exit(IExpression expression) : IStatement
{
    public IExpression Expression = expression;
}