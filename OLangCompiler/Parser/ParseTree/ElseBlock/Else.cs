using OLangCompiler.Parser.ParseTree.Scope;

namespace OLangCompiler.Parser.ParseTree.ElseBlock;

public class Else(IScopeNode scope) : IElse
{
    public IScopeNode Scope = scope;
}