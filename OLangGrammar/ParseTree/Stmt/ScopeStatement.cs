using Lexing;
using OLangGrammar.ParseTree.Scope;

namespace OLangGrammar.ParseTree.Stmt;

public class ScopeStatement(IScopeNode scope) : IStatement
{
    public IScopeNode Scope = scope;
    public SourceSpan Span { get; set; }
}