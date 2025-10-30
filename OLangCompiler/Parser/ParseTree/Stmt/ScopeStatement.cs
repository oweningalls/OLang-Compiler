using OLangCompiler.Parser.ParseTree.Scope;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class ScopeStatement(IScopeNode scope) : IStatement
{
    public IScopeNode Scope = scope;
}