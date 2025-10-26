using OLangCompiler.Parser.ParseTree.Scope;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class ScopeStatement(ScopeNode scope) : IStatementNode
{
    public ScopeNode Scope = scope;
}