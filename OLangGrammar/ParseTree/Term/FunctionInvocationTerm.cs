using Lexing;

namespace OLangGrammar.ParseTree.Term;

public class FunctionInvocationTerm(FunctionInvocation.IFunctionInvocation invocationNode) : ITerm
{
    public FunctionInvocation.IFunctionInvocation InvocationNode = invocationNode;
    public SourceSpan Span { get; set; }
}