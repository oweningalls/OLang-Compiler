namespace OLangAst.Statements;

public class Scope(List<IStatement> statements) : IStatement
{
    public List<IStatement> Statements = statements;
}