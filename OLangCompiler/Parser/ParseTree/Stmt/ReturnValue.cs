using OLangCompiler.Parser.ParseTree.Expression;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class ReturnValue(BaseExpression expression) : IStatement
{
    public BaseExpression Expression = expression;
}