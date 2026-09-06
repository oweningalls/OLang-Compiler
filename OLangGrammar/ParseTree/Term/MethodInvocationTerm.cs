using Lexing;

namespace OLangGrammar.ParseTree.Term;

public class MethodInvocationTerm(MethodInvocation.IMethodInvocation invocationNode) : ITerm
{
    public MethodInvocation.IMethodInvocation InvocationNode = invocationNode;
    public SourceSpan Span { get; set; }
}