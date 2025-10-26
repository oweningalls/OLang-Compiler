using OLangCompiler.Parser.ParseTree.Expression;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class Exit(BaseExpression expression) : IStatement
{
    public BaseExpression Expression = expression;
}