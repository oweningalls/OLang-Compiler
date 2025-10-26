using OLangCompiler.Parser.ParseTree.Scope;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class ScopeStatement(ScopeNode scope) : IStatement
{
    public ScopeNode Scope = scope;
}