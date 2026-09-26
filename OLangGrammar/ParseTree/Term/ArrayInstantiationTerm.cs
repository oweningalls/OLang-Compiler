using Lexing;
using OLangGrammar.ParseTree.Type;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Term;

public class ArrayInstantiationTerm(IType type, IntLiteralToken size) : ITerm
{
    public IType Type = type;
    public IntLiteralToken Size = size;
    public SourceSpan Span { get; set; }
}