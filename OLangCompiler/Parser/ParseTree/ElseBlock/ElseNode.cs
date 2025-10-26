using OLangCompiler.Parser.ParseTree.Scope;

namespace OLangCompiler.Parser.ParseTree.ElseBlock;

public class ElseNode(ScopeNode scope) : IElseNode
{
    public ScopeNode Scope = scope;
}