using OLangCompiler.Parser.ParseTree.Scope;

namespace OLangCompiler.Parser.ParseTree.ElseBlock;

public class Else(ScopeNode scope) : IElse
{
    public ScopeNode Scope = scope;
}