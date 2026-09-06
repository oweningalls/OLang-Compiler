using Lexing;
using OLangGrammar.ParseTree.MethodInvocation;

namespace OLangGrammar.ParseTree.Stmt;

public class MethodInvocationStatement(IMethodInvocation invocationNode) : IStatement
{
    public IMethodInvocation InvocationNode = invocationNode;
    public SourceSpan Span { get; set; }
}